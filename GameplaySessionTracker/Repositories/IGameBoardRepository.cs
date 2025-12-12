using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public interface IGameBoardRepository
    {
        Task<IEnumerable<GameBoard>> GetAll();
        Task<GameBoard?> GetById(Guid id);
        Task Add(GameBoard gameBoard);
        Task Update(GameBoard gameBoard);
        Task Delete(Guid id);
    }
}
