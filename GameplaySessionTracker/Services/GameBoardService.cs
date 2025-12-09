using System;
using System.Collections.Generic;
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
            return repository.GetAll();
        }

        public async Task<GameBoard?> GetById(Guid id)
        {
            return repository.GetById(id);
        }

        public async Task<GameBoard> Create(GameBoard gameBoard)
        {
            repository.Add(gameBoard);
            return gameBoard;
        }

        public async Task StartGame(Guid id, List<Guid> playerIds)
        {
            var gameBoard = await GetById(id) ?? throw new ArgumentException("Session game board not found");
            var state = CreateNewGameState(playerIds);

            await DoTurn(id, gameBoard, state);
        }

        private async Task DoTurn(Guid id, GameBoard gameBoard, GameState state)
        {
            // Do turn rollover if needed
            if (state.TurnPhase == TurnPhase.End)
            {
                state = EndTurn(state);
            }

            // Notify the current player about their turn
            await hubContext.Clients.Group(gameBoard.SessionId.ToString()).
                SendAsync("YourTurn", state.CurrentPlayerId.GetHashCode().ToString(), GetValidActions(state));

            // Update the game board
            gameBoard.Data = SerializeGameState(state);
            await Update(id, gameBoard);
        }

        public async Task Update(Guid id, GameBoard gameBoard)
        {
            repository.Update(gameBoard);

            // Notify all players about the new state
            await hubContext.Clients.Group(gameBoard.SessionId.ToString()).
                SendAsync("SessionGameBoardUpdated");
        }

        public async Task Delete(Guid id)
        {
            repository.Delete(id);
        }

        public async Task PlayerAction(Guid id, GameAction action)
        {
            var sessionGameBoard = await GetById(id) ?? throw new ArgumentException("Session game board not found");
            var state = DeserializeGameState(sessionGameBoard.Data);
            var onType = GameConstants.Spaces[state.Players[state.CurrentPlayerId].BoardPosition].Type;

            state = action switch
            {
                { Type: ActionType.Move } => Move(state),
                { Type: ActionType.Accept } when onType == SpaceType.Land => BuyProperty(state),
                { Type: ActionType.Accept } when onType == SpaceType.Track => BuyTrack(state),
                { Type: ActionType.Accept } when onType == SpaceType.SettlerRents => SettlerRents(state),
                { Type: ActionType.Accept } when onType == SpaceType.LandClaims => LandClaims(state),
                { Type: ActionType.Accept } when onType == SpaceType.Rebellion => StartRebellion(state),
                { Type: ActionType.Accept } when onType == SpaceType.SurveyFees => SurveyFees(state),
                { Type: ActionType.Accept } when onType == SpaceType.RoadbedCosts => RoadbedCosts(state),
                { Type: ActionType.Accept } when onType == SpaceType.EndOfTrack => EndOfTrack(state),
                { Type: ActionType.Accept } when onType == SpaceType.Go => Pass(state),
                { Type: ActionType.Rebellion, Target: CityPair target } => Rebellion(state, target),
                { Type: ActionType.PlaceTrack, Target: CityPair target } => PlaceTrack(state, target),
                { Type: ActionType.Pass } => Pass(state),
                { Type: ActionType.Trade } => state, // TODO: implement RuleEngine.Trade(state),
                _ => throw new ArgumentException("Invalid action type")
            };

            await DoTurn(id, sessionGameBoard, state);
        }
    }
}
