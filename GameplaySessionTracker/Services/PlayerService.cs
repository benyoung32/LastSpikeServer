using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;

namespace GameplaySessionTracker.Services
{
    public class PlayerService(IPlayerRepository repository) : IPlayerService
    {
        public IEnumerable<Player> GetAll()
        {
            return repository.GetAll();
        }

        public Player? GetById(Guid id)
        {
            return repository.GetById(id);
        }

        public Player Create(Player player)
        {
            repository.Add(player);
            return player;
        }

        public void Update(Guid id, Player player)
        {
            repository.Update(player);
        }

        public void Delete(Guid id)
        {
            repository.Delete(id);
        }
    }
}
