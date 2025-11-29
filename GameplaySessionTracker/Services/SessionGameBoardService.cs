using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using Microsoft.AspNetCore.SignalR;
using GameplaySessionTracker.Hubs;
using GameplaySessionTracker.GameRules;

namespace GameplaySessionTracker.Services
{
    public class SessionGameBoardService(
        ISessionGameBoardRepository repository,
        IHubContext<GameHub> hubContext) : ISessionGameBoardService
    {

        public async Task<IEnumerable<SessionGameBoard>> GetAll()
        {
            return repository.GetAll();
        }

        public async Task<SessionGameBoard?> GetById(Guid id)
        {
            return repository.GetById(id);
        }

        public async Task<SessionGameBoard> Create(SessionGameBoard sessionGameBoard)
        {
            repository.Add(sessionGameBoard);
            return sessionGameBoard;
        }

        public async Task Update(Guid id, SessionGameBoard sessionGameBoard)
        {
            repository.Update(sessionGameBoard);
            await hubContext.Clients.Group(sessionGameBoard.SessionId.ToString()).SendAsync("SessionGameBoardUpdated", sessionGameBoard.Data);
        }

        public async Task Delete(Guid id)
        {
            repository.Delete(id);
        }

        public async Task PlayerAction(Guid id, GameAction action)
        {
            var sessionGameBoard = await GetById(id);
            var gameState = RuleEngine.DeserializeGameState(sessionGameBoard.Data);
            gameState = RuleEngine.ApplyAction(gameState, action);
            sessionGameBoard.Data = RuleEngine.SerializeGameState(gameState);
            await Update(id, sessionGameBoard);
        }
    }
}
