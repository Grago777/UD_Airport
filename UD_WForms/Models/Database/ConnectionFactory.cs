using Npgsql;
using System;

namespace UD_WForms.Models.Database
{
    public static class ConnectionFactory
    {
        private static string _connectionString =
            "Host=localhost;Port=5432;Database=aviadb;Username=postgres;Password=Frida";

        public static NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public static NpgsqlConnection CreateConnection(string databaseName = "aviadb")
        {
            var connString = _connectionString.Replace("Database=aviadb", $"Database={databaseName}");
            return new NpgsqlConnection(connString);
        }

        public static void SetConnectionString(string host, string port, string database, string username, string password)
        {
            _connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static string GetConnectionString()
        {
            return _connectionString;
        }

        public static string GetConnectionString(string databaseName)
        {
            return _connectionString.Replace("Database=aviadb", $"Database={databaseName}");
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = CreateConnection("postgres"))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}