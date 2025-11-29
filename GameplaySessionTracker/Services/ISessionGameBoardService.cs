using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Services
{
    public interface ISessionGameBoardService
    {
        Task<IEnumerable<SessionGameBoard>> GetAll();
        Task<SessionGameBoard?> GetById(Guid id);
        Task<SessionGameBoard> Create(SessionGameBoard sessionGameBoard);
        Task Update(Guid id, SessionGameBoard sessionGameBoard);
        Task Delete(Guid id);
        Task PlayerAction(Guid id, GameAction action);
    }
}
