using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using GameplaySessionTracker.Models;
using Microsoft.Data.SqlClient;

namespace GameplaySessionTracker.Repositories
{
    public class GameBoardRepository : IGameBoardRepository
    {
        private readonly string _connectionString;

        public GameBoardRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public IEnumerable<GameBoard> GetAll()
        {
            using var connection = CreateConnection();
            return connection.Query<GameBoard>("SELECT * FROM GameBoards");
        }

        public GameBoard? GetById(Guid id)
        {
            using var connection = CreateConnection();
            return connection.QueryFirstOrDefault<GameBoard>(
                "SELECT * FROM GameBoards WHERE Id = @Id",
                new { Id = id });
        }

        public void Add(GameBoard gameBoard)
        {
            using var connection = CreateConnection();
            connection.Execute(
                "INSERT INTO GameBoards (Id, Description, Data) VALUES (@Id, @Description, @Data)",
                gameBoard);
        }

        public void Update(GameBoard gameBoard)
        {
            using var connection = CreateConnection();
            connection.Execute(
                "UPDATE GameBoards SET Description = @Description, Data = @Data WHERE Id = @Id",
                gameBoard);
        }

        public void Delete(Guid id)
        {
            using var connection = CreateConnection();
            connection.Execute("DELETE FROM GameBoards WHERE Id = @Id", new { Id = id });
        }
    }
}
