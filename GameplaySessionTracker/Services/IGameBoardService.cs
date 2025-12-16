using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.GameRules;

namespace GameplaySessionTracker.Services
{
    public interface IGameBoardService
    {
        Task<IEnumerable<GameBoard>> GetAll();
        Task<GameBoard?> GetById(Guid id);
        Task<GameBoard> Create(GameBoard gameBoard);
        Task Update(Guid id, GameBoard gameBoard);
        Task Delete(Guid id);
        Task PlayerAction(Guid id, GameAction action);
        Task OfferTrade(Guid id, Trade trade);
        Task CloseTradeOffer(Guid id, bool accept);
        Task StartGame(Guid id, List<Guid> playerIds);

    }
}
