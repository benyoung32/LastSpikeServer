using GameplaySessionTracker.GameRules;
using GameplaySessionTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GameplaySessionTracker.Tests.GameRules;

public class RuleEngineTests
{
    private readonly List<Guid> testIds;

    public RuleEngineTests()
    {
        testIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
    }

    [Fact]
    public void CreateNewGameState_InitializesCorrectly()
    {
        var state = RuleEngine.CreateNewGameState(testIds);

        Assert.Equal(testIds.Count, state.Players.Count);
        Assert.Equal(testIds[0], state.CurrentPlayerId);
        Assert.Equal(TurnPhase.Start, state.TurnPhase);
        Assert.Empty(state.Routes);
        Assert.Empty(state.Properties);
        Assert.False(state.IsGameOver);

        foreach (var player in state.Players.Values)
        {
            Assert.Equal(GameConstants.PlayerStartingMoney, player.Money);
            Assert.Equal(0, player.BoardPosition);
        }
    }

    [Fact]
    public void Move_UpdatesPositionAndPhase()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var initialPos = state.Players[state.CurrentPlayerId].BoardPosition;

        var newState = RuleEngine.Move(state);

        var newPos = newState.Players[newState.CurrentPlayerId].BoardPosition;
        Assert.True(newPos >= initialPos + 2);
        Assert.True(newPos <= initialPos + 12); // Max dice roll is 12
        Assert.Equal(TurnPhase.SpaceOption, newState.TurnPhase);
    }

    [Fact]
    public void Move_WrapsAroundBoard_AndAwardsSubsidy()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        // Position player near end of board
        var player = state.Players[state.CurrentPlayerId];
        state.Players[state.CurrentPlayerId] = player with { BoardPosition = GameConstants.Spaces.Count - 1, Money = 1000 };

        var newState = RuleEngine.Move(state);

        var newPlayer = newState.Players[newState.CurrentPlayerId];
        Assert.True(newPlayer.BoardPosition < GameConstants.Spaces.Count);
        Assert.True(newPlayer.Money > 1000); // Should have received subsidy
        Assert.Equal(TurnPhase.SpaceOption, newState.TurnPhase);
    }

    [Fact]
    public void SettlerRents_AddsMoneyBasedOnProperties()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        state.Properties.Add(new Property(City.Vancouver, state.CurrentPlayerId));
        state.Properties.Add(new Property(City.Calgary, state.CurrentPlayerId));
        var initialMoney = state.Players[state.CurrentPlayerId].Money;

        var newState = RuleEngine.SettlerRents(state);

        var newMoney = newState.Players[newState.CurrentPlayerId].Money;
        Assert.Equal(initialMoney + 2000, newMoney); // 1000 per property
        Assert.Equal(TurnPhase.End, newState.TurnPhase);
    }

    [Fact]
    public void RoadbedCosts_DeductsMoneyBasedOnProperties()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        state.Properties.Add(new Property(City.Vancouver, state.CurrentPlayerId));
        var initialMoney = state.Players[state.CurrentPlayerId].Money;

        var newState = RuleEngine.RoadbedCosts(state);

        var newMoney = newState.Players[newState.CurrentPlayerId].Money;
        Assert.Equal(initialMoney - 1000, newMoney); // 1000 per property
        Assert.Equal(TurnPhase.End, newState.TurnPhase);
    }

    [Fact]
    public void SurveyFees_AddsMoneyBasedOnPlayerCount()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var initialMoney = state.Players[state.CurrentPlayerId].Money;

        var newState = RuleEngine.SurveyFees(state);

        var newMoney = newState.Players[newState.CurrentPlayerId].Money;
        Assert.Equal(initialMoney + (testIds.Count * 3000), newMoney);
        Assert.Equal(TurnPhase.End, newState.TurnPhase);
    }

    [Fact]
    public void LandClaims_DeductsMoneyRandomly()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var initialMoney = state.Players[state.CurrentPlayerId].Money;

        var newState = RuleEngine.LandClaims(state);

        var newMoney = newState.Players[newState.CurrentPlayerId].Money;
        Assert.True(newMoney < initialMoney);
        Assert.True(newMoney >= initialMoney - 12000); // Max roll 12 * 1000
        Assert.Equal(TurnPhase.End, newState.TurnPhase);
    }

    [Fact]
    public void BuyProperty_ExhaustDeck()
    {
        // Arrange
        var state = RuleEngine.CreateNewGameState(testIds);
        state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with { Money = 99999999 };

        // Act
        for (var i = 0; i < 45; i++)
        {
            state = RuleEngine.BuyProperty(state);
        }
        // Assert
        // check that there are 5 of each city
        foreach (var city in Enum.GetValues(typeof(City)).Cast<City>())
        {
            var count = state.Properties.Count(p => p.City == city);
            Assert.Equal(5, count);
        }
        // drawing a 6th card should fail
        var newState = RuleEngine.BuyProperty(state);
        Assert.Equal(state, newState);
    }

    [Fact]
    public void BuyProperty_DeductsSpaceCost_AddsProperty()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        // Force player onto a track space to pay cost
        // Assuming space 1 is a track space with cost
        var trackSpaceIndex = GameConstants.Spaces.FindIndex(s => s.Type == SpaceType.Track);
        if (trackSpaceIndex == -1) return; // Should not happen

        state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with { BoardPosition = trackSpaceIndex };
        var initialMoney = state.Players[state.CurrentPlayerId].Money;
        var spaceCost = GameConstants.Spaces[trackSpaceIndex].Cost;

        var newState = RuleEngine.BuyProperty(state);

        Assert.Single(newState.Properties);
        Assert.Equal(newState.CurrentPlayerId, newState.Properties[0].Owner_PID);
        Assert.Equal(initialMoney - spaceCost, newState.Players[newState.CurrentPlayerId].Money);
        Assert.Equal(TurnPhase.End, newState.TurnPhase);
    }

    [Fact]
    public void StartRebellion_NoTargets_ReturnsSameState()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        // No routes with 2 or 3 tracks

        var newState = RuleEngine.StartRebellion(state);

        Assert.Equal(state, newState); // Should not change phase if no targets
    }

    [Fact]
    public void StartRebellion_WithTargets_ChangesPhase()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var cityPair = new CityPair(City.Calgary, City.Vancouver);
        state.Routes.Add(new Route(cityPair, 2)); // Valid target

        var newState = RuleEngine.StartRebellion(state);

        Assert.Equal(TurnPhase.RouteSelect, newState.TurnPhase);
    }

    [Fact]
    public void Rebellion_RemovesTrack()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var cityPair = new CityPair(City.Calgary, City.Vancouver);
        state.Routes.Add(new Route(cityPair, 2));

        var newState = RuleEngine.Rebellion(state, cityPair);

        var route = newState.Routes.Find(r => r.CityPair == cityPair);
        Assert.Equal(1, route.NumTracks);
        Assert.Equal(TurnPhase.End, newState.TurnPhase);
    }

    [Fact]
    public void PlaceTrack_NewRoute_AddsTrackAndProperty()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var cityPair = new CityPair(City.Calgary, City.Vancouver); // Valid pair

        var newState = RuleEngine.PlaceTrack(state, cityPair);

        var route = newState.Routes.Find(r => r.CityPair == cityPair);
        Assert.NotNull(route);
        Assert.Equal(1, route.NumTracks);
        Assert.Single(newState.Properties); // First track gets a property
        Assert.Equal(TurnPhase.End, newState.TurnPhase);
    }

    [Fact]
    public void PlaceTrack_ExistingRoute_AddsTrack()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var cityPair = new CityPair(City.Calgary, City.Vancouver);
        state.Routes.Add(new Route(cityPair, 1));

        var newState = RuleEngine.PlaceTrack(state, cityPair);

        var route = newState.Routes.Find(r => r.CityPair == cityPair);
        Assert.Equal(2, route.NumTracks);
        Assert.Empty(newState.Properties); // Only first track gets property
        Assert.Equal(TurnPhase.End, newState.TurnPhase);
    }

    [Fact]
    public void PlaceTrack_FinishesRoute_Payouts()
    {
        // Setup state where route has 3 tracks and players own properties
        var state = RuleEngine.CreateNewGameState(testIds);
        var cityPair = new CityPair(City.Calgary, City.Vancouver);
        state.Routes.Add(new Route(cityPair, 3));

        // Give players properties
        state.Properties.Add(new Property(City.Vancouver, testIds[0]));
        state.Properties.Add(new Property(City.Calgary, testIds[1]));

        var initialMoney0 = state.Players[testIds[0]].Money;
        var initialMoney1 = state.Players[testIds[1]].Money;

        var newState = RuleEngine.PlaceTrack(state, cityPair);

        var route = newState.Routes.Find(r => r.CityPair == cityPair);
        Assert.Equal(4, route.NumTracks);

        // Check payouts occurred (exact amounts depend on GameConstants, checking increase)
        Assert.True(newState.Players[testIds[0]].Money > initialMoney0);
        Assert.True(newState.Players[testIds[1]].Money > initialMoney1);
    }

    [Fact]
    public void Pass_EndsTurnPhase()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        state = state with { TurnPhase = TurnPhase.SpaceOption };

        var newState = RuleEngine.Pass(state);

        Assert.Equal(TurnPhase.End, newState.TurnPhase);
    }

    [Fact]
    public void EndTurn_AdvancesToNextPlayer()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        state = state with { TurnPhase = TurnPhase.End };
        var initialPlayer = state.CurrentPlayerId;

        var newState = RuleEngine.EndTurn(state);

        Assert.NotEqual(initialPlayer, newState.CurrentPlayerId);
        Assert.Equal(testIds[1], newState.CurrentPlayerId);
        Assert.Equal(TurnPhase.Start, newState.TurnPhase);
    }

    [Fact]
    public void GetValidActions_LandSpace_ReturnsExpected()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        // Find a Land space
        var landIndex = GameConstants.Spaces.FindIndex(s => s.Type == SpaceType.Land);
        state.Players[state.CurrentPlayerId] = state.Players[state.CurrentPlayerId] with { BoardPosition = landIndex };

        var actions = RuleEngine.GetValidActions(state);

        Assert.Contains(ActionType.Ok, actions);
        Assert.Contains(ActionType.Pass, actions);
        Assert.Contains(ActionType.TradeOffer, actions);
    }

    [Fact]
    public void ExecuteTrade_NoPendingTrade_ReturnsOriginalState()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        Assert.Null(state.PendingTrade);

        var newState = RuleEngine.ExecuteTrade(state);

        Assert.Equal(state, newState);
    }

    [Fact]
    public void ExecuteTrade_InvalidTrade_ReturnsOriginalState()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var p1 = testIds[0];
        var p2 = testIds[1];

        // Invalid because p1 doesn't have enough money (cost 999999)
        var trade = new Trade(p1, p2, new List<Property>(), 999999, 0);
        state = state with { PendingTrade = trade };

        var newState = RuleEngine.ExecuteTrade(state);

        // State remains same (including PendingTrade staying there)
        // Wait, RuleEngine.ExecuteTrade returns 'state' which includes the PendingTrade if invalid. 
        // Logic check: RuleEngine.cs:345 returns state. 
        Assert.Same(state.PendingTrade, newState.PendingTrade);
        Assert.Equal(state.Players[p1].Money, newState.Players[p1].Money);
    }

    [Fact]
    public void ExecuteTrade_ValidMoneyTrade_TransfersMoney()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var p1 = testIds[0];
        var p2 = testIds[1];
        var p1Initial = state.Players[p1].Money;
        var p2Initial = state.Players[p2].Money;

        // P1 pays 1000 to P2
        var trade = new Trade(p1, p2, new List<Property>(), 1000, 0);
        state = state with { PendingTrade = trade };

        var newState = RuleEngine.ExecuteTrade(state);

        Assert.Null(newState.PendingTrade);
        Assert.Equal(p1Initial - 1000, newState.Players[p1].Money);
        Assert.Equal(p2Initial + 1000, newState.Players[p2].Money);
    }

    [Fact]
    public void ExecuteTrade_ValidPropertyTrade_TransfersProperties()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var p1 = testIds[0];
        var p2 = testIds[1];

        // Give P1 a property
        var prop = new Property(City.Vancouver, p1);
        state.Properties.Add(prop);

        // Trade prop from P1 to P2
        var trade = new Trade(p1, p2, new List<Property> { prop }, 0, 0);
        state = state with { PendingTrade = trade };

        var newState = RuleEngine.ExecuteTrade(state);

        var tradedProp = newState.Properties.First(p => p.City == City.Vancouver);
        Assert.Equal(p2, tradedProp.Owner_PID);
    }

    [Fact]
    public void ExecuteTrade_ValidComplexTrade_TransfersBoth()
    {
        var state = RuleEngine.CreateNewGameState(testIds);
        var p1 = testIds[0];
        var p2 = testIds[1];
        var p1Initial = state.Players[p1].Money;
        var p2Initial = state.Players[p2].Money;

        var prop1 = new Property(City.Vancouver, p1);
        state.Properties.Add(prop1);

        var trade = new Trade(p1, p2, new List<Property> { prop1 }, 0, 2000);
        state = state with { PendingTrade = trade };

        var newState = RuleEngine.ExecuteTrade(state);

        var tradedProp = newState.Properties.First(p => p.City == City.Vancouver);
        Assert.Equal(p2, tradedProp.Owner_PID);

        Assert.Equal(p1Initial + 2000, newState.Players[p1].Money); // Recvs 2000
        Assert.Equal(p2Initial - 2000, newState.Players[p2].Money); // Pays 2000
    }
}
