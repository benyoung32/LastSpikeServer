using System;
using System.Collections.Generic;
using System.Linq;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Repositories
{
    public class GameBoardRepository : IGameBoardRepository
    {
        private readonly List<GameBoard> _gameBoards = new();

        public IEnumerable<GameBoard> GetAll()
        {
            return _gameBoards;
        }

        public GameBoard? GetById(Guid id)
        {
            return _gameBoards.FirstOrDefault(sgb => sgb.Id == id);
        }

        public void Add(GameBoard gameBoard)
        {
            _gameBoards.Add(gameBoard);
        }

        public void Update(GameBoard gameBoard)
        {
            var existing = GetById(gameBoard.Id);
            if (existing != null)
            {
                existing.SessionId = gameBoard.SessionId;
                existing.Data = gameBoard.Data;
            }
        }

        public void Delete(Guid id)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                _gameBoards.Remove(existing);
            }
        }
    }
}
