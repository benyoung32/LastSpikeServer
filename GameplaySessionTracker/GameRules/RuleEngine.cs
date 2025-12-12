using System.Text.Json;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.GameRules
{
    public record GameState(
        OrderedDictionary<Guid, PlayerState> Players, // the order of this list determines turn order
        List<Route> Routes, // how much track on each route
        List<Property> Properties, // "deck" of properties 
        bool IsGameOver,
        Guid CurrentPlayerId, // which player's turn it is
                              // TODO: add last dice rolled tuple for clientside display
        TurnPhase TurnPhase
    );

    public record PlayerState(
            int Money,
            int BoardPosition, // value from 0->len(Spaces)  
            bool SkipNextTurn // if true, the player will skip their next turn
        );

    public record CityPair(City City1, City City2) : IEnumerable<City>
    {
        public IEnumerator<City> GetEnumerator()
        {
            yield return City1;
            yield return City2;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return $"{City1}, {City2}";
        }
    }

    public record Route(CityPair CityPair, int NumTracks);

    public record Property(City City, Guid Owner_PID);

    public static class RuleEngine
    {

        /// <summary>
        /// Creates a new game state with the given player IDs
        /// </summary>
        /// <param name="playerIDs"></param>
        /// <returns></returns>
        public static GameState CreateNewGameState(List<Guid> playerIDs)
        {
            var state = new GameState(
            new OrderedDictionary<Guid, PlayerState>(),
            new List<Route>(),
            new List<Property>(),
            false,
            playerIDs[0],
            TurnPhase.Start);
            foreach (var p in playerIDs)
            {
                state.Players.Add(p, new PlayerState(GameConstants.PlayerStartingMoney, 0, false));
            }
            return state;
        }

        #region Game Actions
        // every method here advances the game state and the turn phase
        /// <summary>
        /// Moves the current player forward according to a dice roll
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static GameState Move(GameState state)
        {
            var diceRoll = DiceRoll();
            var currentPlayer = state.Players[state.CurrentPlayerId];
            var newBoardPosition = currentPlayer.BoardPosition + diceRoll;
            if (newBoardPosition >= GameConstants.Spaces.Count)
            {
                state = PassGo(state);
                newBoardPosition -= GameConstants.Spaces.Count;
            }
            state.Players[state.CurrentPlayerId] = currentPlayer with
            {
                BoardPosition = newBoardPosition
            };
            return state with
            {
                TurnPhase = TurnPhase.SpaceOption
            };
        }

        public static GameState SettlerRents(GameState state)
        {
            state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with
            {
                Money = state.Players[state.CurrentPlayerId].Money +
                state.Properties.Count(p => p.Owner_PID == state.CurrentPlayerId) * 1000
            };
            return state with
            {
                TurnPhase = TurnPhase.End
            };
        }

        public static GameState RoadbedCosts(GameState state)
        {
            state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with
            {
                Money = state.Players[state.CurrentPlayerId].Money -
                state.Properties.Count(p => p.Owner_PID == state.CurrentPlayerId) * 1000
            };
            return state with
            {
                TurnPhase = TurnPhase.End
            };
        }

        public static GameState SurveyFees(GameState state)
        {
            state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with
            {
                Money = state.Players[state.CurrentPlayerId].Money +
                state.Players.Count * 3000
            };
            return state with
            {
                TurnPhase = TurnPhase.End
            };
        }

        public static GameState LandClaims(GameState state)
        {
            state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with
            {
                Money = state.Players[state.CurrentPlayerId].Money -
                DiceRoll() * 1000
            };
            return state with
            {
                TurnPhase = TurnPhase.End
            };
        }
        /// <summary>
        /// Draws a property from the deck of 5 of each city, removing it from the deck and adding it to the player's properties
        /// </summary>
        /// <param name="state"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        public static GameState BuyProperty(GameState state)
        {
            PaySpaceCost(state);
            state = DrawProperty(state);
            return state with
            {
                TurnPhase = TurnPhase.End
            };
        }

        public static GameState StartRebellion(GameState state)
        {
            if (GetRebellionTargets(state).Count == 0)
            {
                return state;
            }

            return state with
            {
                TurnPhase = TurnPhase.RouteSelect
            };
        }

        /// <summary>
        /// Removes 1 track from the specified route. Routes must have between 2 and 3 tracks to be a valid target.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static GameState Rebellion(GameState state, CityPair target)
        {
            // Validate that the route is a valid rebellion target
            if (!GetRebellionTargets(state).Contains(target))
            {
                return state;
            }

            state.Routes[state.Routes.FindIndex(route => route.CityPair == target)] = new Route(target, state.Routes.FirstOrDefault<Route>(route => route.CityPair == target)!.NumTracks - 1);
            return state with
            {
                TurnPhase = TurnPhase.End
            };
        }

        public static GameState BuyTrack(GameState state)
        {
            state = PaySpaceCost(state);
            return state with
            {
                TurnPhase = TurnPhase.RouteSelect
            };
        }

        /// <summary>
        /// Adds 1 track to the specified route, checks if route is finished.
        /// If a track is laid on a currently empty route, the current player is awarded a property
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static GameState PlaceTrack(GameState state, CityPair target)
        {
            // is this a valid target?
            if (!GameConstants.ValidCityPairs.Contains(target))
            {
                return state; // do nothing
            }
            var route = state.Routes.Find(route => route.CityPair == target) ?? new Route(target, 0);
            bool firstTrack = false;
            // if this is the first time track has been added to this route, now add it to the list
            if (!state.Routes.Any(route => route.CityPair == target))
            {
                state.Routes.Add(route);
                firstTrack = true;
            }
            var targetRouteIndex = state.Routes.FindIndex(route => route.CityPair == target);

            // is this route already full?
            if (route.NumTracks == 4)
            {
                return state; // do nothing
            }

            state.Routes[targetRouteIndex] = route with
            {
                NumTracks = route.NumTracks + 1
            };

            // if this was the first track laid, give the current player a new property
            if (firstTrack)
            {
                state = BuyProperty(state);
            }

            // if the route is now full, award players who own properties on the route
            if (route.NumTracks == 4)
            {
                state = FinishRoute(state, target);
            }

            return state with
            {
                TurnPhase = TurnPhase.End
            };
        }

        public static GameState EndOfTrack(GameState state)
        {
            state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with
            {
                SkipNextTurn = true
            };
            return state with
            {
                TurnPhase = TurnPhase.End
            };
        }

        public static GameState Pass(GameState state)
        {
            return state with
            {
                TurnPhase = TurnPhase.End
            };
        }

        public static GameState EndTurn(GameState state)
        {
            var playerIds = state.Players.Keys.ToList();
            var currentPlayerIndex = playerIds.FindIndex(p => p == state.CurrentPlayerId);
            var nextPlayerId = playerIds[(currentPlayerIndex + 1) % playerIds.Count];

            return state with
            {
                TurnPhase = TurnPhase.Start,
                CurrentPlayerId = nextPlayerId
            };
        }

        #endregion

        #region Helpers

        public static GameState PassGo(GameState state)
        {
            state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with
            {
                Money = state.Players[state.CurrentPlayerId].Money + GameConstants.CPRSubsidy
            };
            return state;
        }

        private static GameState DrawProperty(GameState state)
        {
            // Create a full deck of 5 of each city
            var deck = new List<City>();
            foreach (City city in Enum.GetValues<City>())
            {
                for (int i = 0; i < 5; i++)
                {
                    deck.Add(city);
                }
            }

            // Remove properties already present in state.Properties
            foreach (var property in state.Properties)
            {
                deck.Remove(property.City);
            }

            // If deck is empty, return state unchanged
            if (deck.Count == 0)
            {
                return state;
            }

            // Randomly select one city from the remaining deck
            // and add the new property to the game state
            state.Properties.Add(new Property(deck[new Random().Next(deck.Count)], state.CurrentPlayerId));
            return state;
        }

        private static int DiceRoll()
        {
            return new Random().Next(2, 13);
        }

        private static GameState PaySpaceCost(GameState state)
        {
            var currentPlayer = state.Players[state.CurrentPlayerId];
            var currentSpace = GameConstants.Spaces[currentPlayer.BoardPosition];
            if (currentSpace.Type != SpaceType.Track)
                return state;
            state.Players[state.CurrentPlayerId] = currentPlayer with
            {
                Money = currentPlayer.Money - currentSpace.Cost
            };
            return state;
        }

        /// <summary>
        /// Returns a list of routes which have between 2 and 3 tracks
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static List<CityPair> GetRebellionTargets(GameState state)
        {
            return [.. state.Routes.Where(route => route.NumTracks > 1 && route.NumTracks < 4)
            .Select(route => route.CityPair)];
        }

        /// <summary>
        /// Completes the game over state, 
        /// setting IsGameOver to true and 
        /// awarding the player who laid the final track of the game with the Last Spike bonus
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static GameState ProcessGameOver(GameState state)
        {
            // the player that finishes the game gets bonus
            state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with
            {
                Money = state.Players[state.CurrentPlayerId].Money + GameConstants.LastSpikeBonus
            };
            return state with
            {
                IsGameOver = true
            };
        }

        /// <summary>
        /// Awards money to players who own properties on the finished route, check game over state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="finished"></param>
        /// <returns></returns>
        private static GameState FinishRoute(GameState state, CityPair finished)
        {
            var num_owned = new Dictionary<Guid, Dictionary<City, int>>();
            var awards = new Dictionary<Guid, int>();
            var newPlayers = new List<Player>();

            var ownedProperties = state.Properties.Where(property =>
                property.City == finished.City1 || property.City == finished.City2);

            foreach (var property in ownedProperties)
            {
                if (!num_owned.ContainsKey(property.Owner_PID))
                    num_owned[property.Owner_PID] = new Dictionary<City, int>();

                if (!num_owned[property.Owner_PID].ContainsKey(property.City))
                    num_owned[property.Owner_PID][property.City] = 0;

                num_owned[property.Owner_PID][property.City] += 1;
            }

            foreach (var city in finished)
                foreach (var playerId in state.Players.Keys)
                {
                    if (!awards.ContainsKey(playerId))
                        awards[playerId] = 0;

                    int count = 0;
                    if (num_owned.ContainsKey(playerId) && num_owned[playerId].ContainsKey(city))
                        count = num_owned[playerId][city];

                    awards[playerId] += GameConstants.CityValues[city][count];
                }

            foreach (var playerId in awards.Keys)
            {
                state.Players[playerId] = state.Players[playerId] with
                {
                    Money = state.Players[playerId].Money + awards[playerId]
                };
            }

            if (IsGameOver(state))
            {
                state = ProcessGameOver(state);
            }

            return state;
        }

        public static List<ActionType> GetValidActions(GameState state)
        {
            var landedOn = GameConstants.Spaces[state.Players[state.CurrentPlayerId].BoardPosition];
            return landedOn.Type switch
            {
                SpaceType.Land => [ActionType.Accept, ActionType.Pass, ActionType.Trade],
                SpaceType.Track when state.TurnPhase == TurnPhase.SpaceOption => [ActionType.Accept, ActionType.Trade, ActionType.PlaceTrack],
                SpaceType.Track when state.TurnPhase == TurnPhase.RouteSelect => [ActionType.PlaceTrack],
                SpaceType.Rebellion when state.TurnPhase == TurnPhase.SpaceOption => [ActionType.Accept],
                SpaceType.Rebellion when state.TurnPhase == TurnPhase.RouteSelect => [ActionType.Rebellion],
                SpaceType.SettlerRents => [ActionType.Accept],
                SpaceType.LandClaims => [ActionType.Accept],
                SpaceType.SurveyFees => [ActionType.Accept],
                SpaceType.EndOfTrack => [ActionType.Accept],
                SpaceType.RoadbedCosts => [ActionType.Accept],
                SpaceType.Go => [ActionType.Accept],
                _ => throw new ArgumentException("Invalid space type")
            };


        }

        private static bool IsGameOver(GameState state)
        {
            // Get all completed routes (routes with 4 tracks)
            var completedRoutes = state.Routes.Where(route => route.NumTracks == 4).ToList();

            if (completedRoutes.Count < 4)
                return false;

            // Build adjacency list for completed routes
            var graph = new Dictionary<City, List<City>>();

            foreach (var route in completedRoutes)
            {
                if (!graph.ContainsKey(route.CityPair.City1))
                    graph[route.CityPair.City1] = new List<City>();
                if (!graph.ContainsKey(route.CityPair.City2))
                    graph[route.CityPair.City2] = new List<City>();

                graph[route.CityPair.City1].Add(route.CityPair.City2);
                graph[route.CityPair.City2].Add(route.CityPair.City1);
            }

            // Check if Vancouver and Montreal are in the graph
            if (!graph.ContainsKey(City.Vancouver) || !graph.ContainsKey(City.Montreal))
                return false;

            // BFS to find path from Vancouver to Montreal
            var queue = new Queue<City>();
            var visited = new HashSet<City>();

            queue.Enqueue(City.Vancouver);
            visited.Add(City.Vancouver);

            while (queue.Count > 0)
            {
                var currentCity = queue.Dequeue();

                if (currentCity == City.Montreal)
                    return true;

                foreach (var neighbor in graph[currentCity])
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            return false;
        }

        public static string SerializeGameState(GameState state)
        {
            return JsonSerializer.Serialize(state);
        }

        public static GameState DeserializeGameState(string json)
        {
            return JsonSerializer.Deserialize<GameState>(json);
        }
    }
    #endregion
}