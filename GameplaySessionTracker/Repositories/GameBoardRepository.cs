using Dapper;
using GameplaySessionTracker.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace GameplaySessionTracker.Repositories
{
    // TODO: implement storage optimization. 
    // The data can be compressed and decompressed in the repository layer to save storage in the SQL database
    public class GameBoardRepository(string connectionString, IMemoryCache cache) : IGameBoardRepository
    {
        private SqlConnection CreateConnection() => new(connectionString);

        public async Task<IEnumerable<GameBoard>> GetAll()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<GameBoard>("SELECT * FROM GameBoards");
        }

        public async Task<GameBoard?> GetById(Guid id)
        {
            var cacheKey = $"GameBoard_{id}";
            if (cache.TryGetValue(cacheKey, out GameBoard? cachedBoard))
            {
                return cachedBoard;
            }

            using var connection = CreateConnection();
            var board = await connection.QueryFirstOrDefaultAsync<GameBoard>("SELECT * FROM GameBoards WHERE Id = @Id", new { Id = id });

            if (board != null)
            {
                cache.Set(cacheKey, board, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });
            }

            return board;
        }

        public async Task Add(GameBoard gameBoard)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "INSERT INTO GameBoards (Id, SessionId, Data) VALUES (@Id, @SessionId, @Data)",
                gameBoard);

            var cacheKey = $"GameBoard_{gameBoard.Id}";
            cache.Set(cacheKey, gameBoard, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });
        }

        public async Task Update(GameBoard gameBoard)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE GameBoards SET SessionId = @SessionId, Data = @Data WHERE Id = @Id",
                gameBoard);

            var cacheKey = $"GameBoard_{gameBoard.Id}";
            cache.Set(cacheKey, gameBoard, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });
        }

        public async Task Delete(Guid id)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync("DELETE FROM GameBoards WHERE Id = @Id", new { Id = id });

            var cacheKey = $"GameBoard_{id}";
            cache.Remove(cacheKey);
        }
    }
}
