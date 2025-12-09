using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public interface IGameBoardRepository
    {
        IEnumerable<GameBoard> GetAll();
        GameBoard? GetById(Guid id);
        void Add(GameBoard gameBoard);
        void Update(GameBoard gameBoard);
        void Delete(Guid id);
    }
}
