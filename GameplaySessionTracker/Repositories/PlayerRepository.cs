using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using GameplaySessionTracker.Models;
using Microsoft.Data.SqlClient;

namespace GameplaySessionTracker.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly string _connectionString;

        public PlayerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public IEnumerable<Player> GetAll()
        {
            using var connection = CreateConnection();
            return connection.Query<Player>("SELECT * FROM Players");
        }

        public Player? GetById(Guid id)
        {
            using var connection = CreateConnection();
            return connection.QueryFirstOrDefault<Player>(
                "SELECT * FROM Players WHERE Id = @Id",
                new { Id = id });
        }

        public void Add(Player player)
        {
            using var connection = CreateConnection();
            connection.Execute(
                "INSERT INTO Players (Id, Name, Alias) VALUES (@Id, @Name, @Alias)",
                player);
        }

        public void Update(Player player)
        {
            using var connection = CreateConnection();
            connection.Execute(
                "UPDATE Players SET Name = @Name, Alias = @Alias WHERE Id = @Id",
                player);
        }

        public void Delete(Guid id)
        {
            using var connection = CreateConnection();
            connection.Execute("DELETE FROM Players WHERE Id = @Id", new { Id = id });
        }
    }
}
