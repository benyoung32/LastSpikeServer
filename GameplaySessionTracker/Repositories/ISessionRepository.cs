using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public interface ISessionRepository
    {
        IEnumerable<SessionData> GetAll();
        SessionData? GetById(Guid id);
        void Add(SessionData session);
        void Update(SessionData session);
        void Delete(Guid id);
    }
}
