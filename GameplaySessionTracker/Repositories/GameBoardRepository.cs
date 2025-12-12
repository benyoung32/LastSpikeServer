using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Linq;
using Dapper;
using GameplaySessionTracker.Models;
using Microsoft.Data.SqlClient;

namespace GameplaySessionTracker.Repositories
{
    public class GameBoardRepository(string connectionString) : IGameBoardRepository
    {
        private SqlConnection CreateConnection() => new(connectionString);

        public async Task<IEnumerable<GameBoard>> GetAll()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<GameBoard>("SELECT * FROM GameBoards");
        }

        public async Task<GameBoard?> GetById(Guid id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<GameBoard>("SELECT * FROM GameBoards WHERE Id = @Id", new { Id = id });
        }

        public async Task Add(GameBoard gameBoard)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "INSERT INTO GameBoards (Id, SessionId, Data) VALUES (@Id, @SessionId, @Data)",
                gameBoard);
        }

        public async Task Update(GameBoard gameBoard)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE GameBoards SET SessionId = @SessionId, Data = @Data WHERE Id = @Id",
                gameBoard);
        }

        public async Task Delete(Guid id)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync("DELETE FROM GameBoards WHERE Id = @Id", new { Id = id });
        }
    }
}
