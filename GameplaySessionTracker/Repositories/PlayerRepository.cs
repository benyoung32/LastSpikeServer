using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using GameplaySessionTracker.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace GameplaySessionTracker.Repositories
{
    public class PlayerRepository(string connectionString, IMemoryCache cache) : IPlayerRepository
    {

        private IDbConnection CreateConnection() => new SqlConnection(connectionString);

        public IEnumerable<Player> GetAll()
        {
            using var connection = CreateConnection();
            return connection.Query<Player>("SELECT * FROM Players");
        }

        public Player? GetById(Guid id)
        {
            var cacheKey = $"Player_{id}";
            if (cache.TryGetValue(cacheKey, out Player? cachedPlayer))
            {
                return cachedPlayer;
            }

            using var connection = CreateConnection();
            var player = connection.QueryFirstOrDefault<Player>(
                "SELECT * FROM Players WHERE Id = @Id",
                new { Id = id });

            if (player != null)
            {
                cache.Set(cacheKey, player, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });
            }

            return player;
        }

        public void Add(Player player)
        {
            using var connection = CreateConnection();
            connection.Execute(
                "INSERT INTO Players (Id, Name) VALUES (@Id, @Name)",
                player);

            var cacheKey = $"Player_{player.Id}";
            cache.Set(cacheKey, player, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });
        }

        public void Update(Player player)
        {
            using var connection = CreateConnection();
            connection.Execute(
                "UPDATE Players SET Name = @Name WHERE Id = @Id",
                player);

            var cacheKey = $"Player_{player.Id}";
            cache.Set(cacheKey, player, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });
        }

        public void Delete(Guid id)
        {
            using var connection = CreateConnection();
            connection.Execute("DELETE FROM Players WHERE Id = @Id", new { Id = id });

            var cacheKey = $"Player_{id}";
            cache.Remove(cacheKey);
        }
    }
}
