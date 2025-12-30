using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public interface IPlayerRepository
    {
        IEnumerable<Player> GetAll();
        Player? GetById(Guid id);
        void Add(Player player);
        void Update(Player player);
        void Delete(Guid id);
    }
}
