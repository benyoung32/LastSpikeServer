using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;

namespace GameplaySessionTracker.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _repository;

        public PlayerService(IPlayerRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Player> GetAll()
        {
            return _repository.GetAll();
        }

        public Player? GetById(Guid id)
        {
            return _repository.GetById(id);
        }

        public Player Create(Player player)
        {
            _repository.Add(player);
            return player;
        }

        public void Update(Guid id, Player player)
        {
            _repository.Update(player);
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }
    }
}
