using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using GameplaySessionTracker.Models;
using Microsoft.Data.SqlClient;

namespace GameplaySessionTracker.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly string _connectionString;

        public SessionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<SessionData>> GetAll()
        {
            using var connection = CreateConnection();
            var sessions = (await connection.QueryAsync<SessionData>("SELECT * FROM Sessions")).ToList();

            return sessions;
        }

        public async Task<SessionData?> GetById(Guid id)
        {
            using var connection = CreateConnection();
            var session = await connection.QueryFirstOrDefaultAsync<SessionData>(
                "SELECT * FROM Sessions WHERE Id = @Id",
                new { Id = id });
            return session;
        }

        public async Task Add(SessionData session)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "INSERT INTO Sessions (Id, Description, BoardId, StartTime, EndTime, PlayerIds) VALUES (@Id, @Description, @BoardId, @StartTime, @EndTime, @PlayerIds)",
                session);
        }

        public async Task Update(SessionData session)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE Sessions SET Description = @Description, BoardId = @BoardId, StartTime = @StartTime, EndTime = @EndTime, PlayerIds = @PlayerIds WHERE Id = @Id",
                session);

        }

        public async Task Delete(Guid id)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync("DELETE FROM Sessions WHERE Id = @Id", new { Id = id });
        }
    }
}
