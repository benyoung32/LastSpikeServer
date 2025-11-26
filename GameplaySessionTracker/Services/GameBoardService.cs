using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;

namespace GameplaySessionTracker.Services
{
    public class GameBoardService : IGameBoardService
    {
        private readonly IGameBoardRepository _repository;

        public GameBoardService(IGameBoardRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<GameBoard> GetAll()
        {
            return _repository.GetAll();
        }

        public GameBoard? GetById(Guid id)
        {
            return _repository.GetById(id);
        }

        public GameBoard Create(GameBoard gameBoard)
        {
            _repository.Add(gameBoard);
            return gameBoard;
        }

        public void Update(Guid id, GameBoard gameBoard)
        {
            _repository.Update(gameBoard);
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }
    }
}
