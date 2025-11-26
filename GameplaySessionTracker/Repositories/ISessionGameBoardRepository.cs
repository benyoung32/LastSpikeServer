using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public interface ISessionGameBoardRepository
    {
        IEnumerable<SessionGameBoard> GetAll();
        SessionGameBoard? GetById(Guid id);
        void Add(SessionGameBoard sessionGameBoard);
        void Update(SessionGameBoard sessionGameBoard);
        void Delete(Guid id);
    }
}
