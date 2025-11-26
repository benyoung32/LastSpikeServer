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

        public IEnumerable<SessionData> GetAll()
        {
            using var connection = CreateConnection();
            var sessions = connection.Query<SessionData>("SELECT * FROM Sessions").ToList();

            // Load PlayerIds for each session
            foreach (var session in sessions)
            {
                session.PlayerIds = connection.Query<Guid>(
                    "SELECT PlayerId FROM SessionPlayers WHERE SessionId = @SessionId",
                    new { SessionId = session.Id }).ToList();
            }

            return sessions;
        }

        public SessionData? GetById(Guid id)
        {
            using var connection = CreateConnection();
            var session = connection.QueryFirstOrDefault<SessionData>(
                "SELECT * FROM Sessions WHERE Id = @Id",
                new { Id = id });

            if (session != null)
            {
                session.PlayerIds = connection.Query<Guid>(
                    "SELECT PlayerId FROM SessionPlayers WHERE SessionId = @SessionId",
                    new { SessionId = id }).ToList();
            }

            return session;
        }

        public void Add(SessionData session)
        {
            using var connection = CreateConnection();
            connection.Execute(
                "INSERT INTO Sessions (Id, Description, BoardId) VALUES (@Id, @Description, @BoardId)",
                session);

            // Add player associations
            foreach (var playerId in session.PlayerIds)
            {
                connection.Execute(
                    "INSERT INTO SessionPlayers (SessionId, PlayerId) VALUES (@SessionId, @PlayerId)",
                    new { SessionId = session.Id, PlayerId = playerId });
            }
        }

        public void Update(SessionData session)
        {
            using var connection = CreateConnection();
            connection.Execute(
                "UPDATE Sessions SET Description = @Description, BoardId = @BoardId WHERE Id = @Id",
                session);

            // Remove existing player associations
            connection.Execute(
                "DELETE FROM SessionPlayers WHERE SessionId = @SessionId",
                new { SessionId = session.Id });

            // Add new player associations
            foreach (var playerId in session.PlayerIds)
            {
                connection.Execute(
                    "INSERT INTO SessionPlayers (SessionId, PlayerId) VALUES (@SessionId, @PlayerId)",
                    new { SessionId = session.Id, PlayerId = playerId });
            }
        }

        public void Delete(Guid id)
        {
            using var connection = CreateConnection();
            // SessionPlayers will be deleted automatically due to ON DELETE CASCADE
            connection.Execute("DELETE FROM Sessions WHERE Id = @Id", new { Id = id });
        }
    }
}
