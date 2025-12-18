using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;
using GameplaySessionTracker.GameRules;

using static GameplaySessionTracker.GameRules.RuleEngine;

namespace GameplaySessionTracker.Services
{
    public class GameBoardService(
        IGameBoardRepository repository,
        IHubContext<GameHub> hubContext) : IGameBoardService
    {

        public async Task<IEnumerable<GameBoard>> GetAll()
        {
            return await repository.GetAll();
        }

        public async Task<GameBoard?> GetById(Guid id)
        {
            return await repository.GetById(id);
        }

        public async Task<GameBoard> Create(GameBoard gameBoard)
        {
            await repository.Add(gameBoard);
            return gameBoard;
        }

        public async Task StartGame(Guid id, List<Guid> playerIds)
        {
            var gameBoard = await GetById(id) ?? throw new ArgumentException("Session game board not found");
            var state = CreateNewGameState(playerIds);

            gameBoard.Data = SerializeGameState(state);
            await Update(id, gameBoard);
            await ProcessTurn(id, gameBoard, state);
        }


        public async Task Update(Guid id, GameBoard gameBoard)
        {
            await repository.Update(gameBoard);

            // Notify all players about the new state
            await hubContext.Clients.Group(gameBoard.SessionId.ToString()).
                SendAsync("GameBoardUpdated");
        }

        public async Task Delete(Guid id)
        {
            await repository.Delete(id);
        }

        public async Task PlayerAction(Guid id, GameAction action)
        {
            var sessionGameBoard = await GetById(id) ?? throw new ArgumentException("Session game board not found");
            var state = DeserializeGameState(sessionGameBoard.Data);
            var onType = GameConstants.Spaces[state.Players[state.CurrentPlayerId].BoardPosition].Type;
            // TODO: log player action to session history to display a game history
            if (state.TurnPhase == TurnPhase.Start)
            {
                state = Move(state);
            }

            else state = action switch
            {
                { Type: ActionType.Buy } when onType == SpaceType.Land => BuyProperty(state),
                { Type: ActionType.Buy } when onType == SpaceType.Track => BuyTrack(state),
                { Type: ActionType.Ok } when onType == SpaceType.SettlerRents => SettlerRents(state),
                { Type: ActionType.Roll } when onType == SpaceType.LandClaims => LandClaims(state),
                { Type: ActionType.Ok } when onType == SpaceType.Rebellion => StartRebellion(state),
                { Type: ActionType.Ok } when onType == SpaceType.SurveyFees => SurveyFees(state),
                { Type: ActionType.Ok } when onType == SpaceType.RoadbedCosts => RoadbedCosts(state),
                { Type: ActionType.Ok } when onType == SpaceType.EndOfTrack => EndOfTrack(state),
                { Type: ActionType.Ok } when onType == SpaceType.Go => PassGo(state),
                { Type: ActionType.Ok } when onType == SpaceType.Scandal => Scandal(state),
                { Type: ActionType.Rebellion, Target: CityPair target } => Rebellion(state, target),
                { Type: ActionType.PlaceTrack, Target: CityPair target } => PlaceTrack(state, target),
                { Type: ActionType.Pass } => Pass(state),
                _ => throw new ArgumentException("Invalid action type")
            };

            await ProcessTurn(id, sessionGameBoard, state);
        }

        public async Task OfferTrade(Guid id, Trade trade)
        {
            var sessionGameBoard = await GetById(id) ?? throw new ArgumentException("Session game board not found");
            var state = DeserializeGameState(sessionGameBoard.Data);

            await ProcessTurn(id, sessionGameBoard, state with { PendingTrade = trade });
        }
        public async Task CloseTradeOffer(Guid id, bool accept)
        {
            var sessionGameBoard = await GetById(id) ?? throw new ArgumentException("Session game board not found");
            var state = DeserializeGameState(sessionGameBoard.Data);
            if (accept && IsTradeValid(state, state.PendingTrade))
            {
                state = ExecuteTrade(state);
            }

            else
            {
                state = state with { PendingTrade = null };
            }
            await ProcessTurn(id, sessionGameBoard, state);
        }

        private async Task ProcessTurn(Guid id, GameBoard gameBoard, GameState state)
        {
            // TODO: implement some sort of action tracker to enable post game history
            // Do turn rollover if needed
            if (state.TurnPhase == TurnPhase.End)
            {
                state = EndTurn(state);
            }
            // Update the game board
            gameBoard.Data = SerializeGameState(state);
            await Update(id, gameBoard);
        }

    }
}
