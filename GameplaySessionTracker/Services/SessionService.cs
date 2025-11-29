using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;

namespace GameplaySessionTracker.Services
{
    public class SessionService(ISessionRepository repository) : ISessionService
    {
        public IEnumerable<SessionData> GetAll()
        {
            return repository.GetAll();
        }

        public SessionData? GetById(Guid id)
        {
            return repository.GetById(id);
        }

        public SessionData Create(SessionData session)
        {
            repository.Add(session);
            return session;
        }

        public void Update(Guid id, SessionData session)
        {
            repository.Update(session);
        }

        public void Delete(Guid id)
        {
            repository.Delete(id);
        }
    }
}
