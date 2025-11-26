using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;

namespace GameplaySessionTracker.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _repository;

        public SessionService(ISessionRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<SessionData> GetAll()
        {
            return _repository.GetAll();
        }

        public SessionData? GetById(Guid id)
        {
            return _repository.GetById(id);
        }

        public SessionData Create(SessionData session)
        {
            _repository.Add(session);
            return session;
        }

        public void Update(Guid id, SessionData session)
        {
            _repository.Update(session);
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }
    }
}
