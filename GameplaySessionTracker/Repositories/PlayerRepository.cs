using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using GameplaySessionTracker.Models;
using Microsoft.Data.SqlClient;

namespace GameplaySessionTracker.Repositories
{
    public class PlayerRepository(string connectionString) : IPlayerRepository
    {

        private IDbConnection CreateConnection() => new SqlConnection(connectionString);

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
                "INSERT INTO Players (Id, Name) VALUES (@Id, @Name)",
                player);
        }

        public void Update(Player player)
        {
            using var connection = CreateConnection();
            connection.Execute(
                "UPDATE Players SET Name = @Name WHERE Id = @Id",
                player);
        }

        public void Delete(Guid id)
        {
            using var connection = CreateConnection();
            connection.Execute("DELETE FROM Players WHERE Id = @Id", new { Id = id });
        }
    }
}
