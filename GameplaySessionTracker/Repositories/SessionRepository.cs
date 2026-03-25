using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using GameplaySessionTracker.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace GameplaySessionTracker.Repositories
{
    public class SessionRepository(string connectionString, IMemoryCache cache) : ISessionRepository
    {
        private SqlConnection CreateConnection() => new(connectionString);

        public async Task<IEnumerable<SessionData>> GetAll()
        {
            using var connection = CreateConnection();
            var sqlQuery = @"
                SELECT * FROM Sessions;
                SELECT * FROM SessionPlayers;";

            using var multi = await connection.QueryMultipleAsync(sqlQuery);
            var sessions = (await multi.ReadAsync<SessionData>()).ToList();
            var sessionPlayers = (await multi.ReadAsync<dynamic>()).ToList();

            var playerLookup = sessionPlayers
                .GroupBy(x => (Guid)x.SessionId)
                .ToDictionary(g => g.Key, g => g.Select(x => (Guid)x.PlayerId).ToList());

            foreach (var session in sessions)
            {
                session.PlayerIds = playerLookup.TryGetValue(session.Id, out var pIds) ? pIds : [];
            }

            return sessions;
        }

        public async Task<SessionData?> GetById(Guid id)
        {
            var cacheKey = $"Session_{id}";
            if (cache.TryGetValue(cacheKey, out SessionData? cachedSession))
            {
                return cachedSession;
            }

            using var connection = CreateConnection();
            var sql = @"
                SELECT * FROM Sessions WHERE Id = @Id;
                SELECT PlayerId FROM SessionPlayers WHERE SessionId = @Id;";

            using var multi = await connection.QueryMultipleAsync(sql, new { Id = id });
            var session = await multi.ReadFirstOrDefaultAsync<SessionData>();

            if (session != null)
            {
                var playerIds = (await multi.ReadAsync<Guid>()).ToList();
                session.PlayerIds = playerIds;

                cache.Set(cacheKey, session, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });
            }

            return session;
        }

        public async Task Add(SessionData session)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    "INSERT INTO Sessions (Id, Description, BoardId, StartTime, EndTime) VALUES (@Id, @Description, @BoardId, @StartTime, @EndTime)",
                    session, transaction);

                if (session.PlayerIds != null && session.PlayerIds.Any())
                {
                    var sessionPlayers = session.PlayerIds.Select(pid => new { SessionId = session.Id, PlayerId = pid });
                    await connection.ExecuteAsync(
                        "INSERT INTO SessionPlayers (SessionId, PlayerId) VALUES (@SessionId, @PlayerId)",
                        sessionPlayers, transaction);
                }

                transaction.Commit();

                var cacheKey = $"Session_{session.Id}";
                cache.Set(cacheKey, session, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task Update(SessionData session)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    "UPDATE Sessions SET Description = @Description, BoardId = @BoardId, StartTime = @StartTime, EndTime = @EndTime WHERE Id = @Id",
                    session, transaction);

                // Update players: Delete existing and re-insert
                await connection.ExecuteAsync("DELETE FROM SessionPlayers WHERE SessionId = @Id", new { session.Id }, transaction);

                if (session.PlayerIds != null && session.PlayerIds.Count > 0)
                {
                    var sessionPlayers = session.PlayerIds.Select(pid => new { SessionId = session.Id, PlayerId = pid });
                    await connection.ExecuteAsync(
                        "INSERT INTO SessionPlayers (SessionId, PlayerId) VALUES (@SessionId, @PlayerId)",
                        sessionPlayers, transaction);
                }

                transaction.Commit();

                var cacheKey = $"Session_{session.Id}";
                cache.Set(cacheKey, session, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task Delete(Guid id)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync("DELETE FROM Sessions WHERE Id = @Id", new { Id = id });

            var cacheKey = $"Session_{id}";
            cache.Remove(cacheKey);
        }

        public async Task<IEnumerable<Player>> GetSessionPlayers(Guid sessionId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT p.*
                FROM Players p
                INNER JOIN SessionPlayers sp ON p.Id = sp.PlayerId
                WHERE sp.SessionId = @SessionId";

            return await connection.QueryAsync<Player>(sql, new { sessionId });
        }
    }
}
