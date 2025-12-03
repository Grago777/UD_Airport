using Npgsql;
using System;

namespace UD_WForms.Models.Database
{
    public static class ConnectionFactory
    {
        private static string _connectionString =
            "Host=localhost;Port=5432;Database=aviadb;Username=postgres;Password=Frida";

        public static NpgsqlConnection CreateConnection(string databaseName = "aviadb")
        {
            var baseConnString = _connectionString;
            var parts = baseConnString.Split(';');
            var newParts = new System.Collections.Generic.List<string>();

            foreach (var part in parts)
            {
                if (part.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                {
                    newParts.Add($"Database={databaseName}");
                }
                else
                {
                    newParts.Add(part);
                }
            }

            return new NpgsqlConnection(string.Join(";", newParts));
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
            var parts = _connectionString.Split(';');
            var newParts = new System.Collections.Generic.List<string>();

            foreach (var part in parts)
            {
                if (part.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                {
                    newParts.Add($"Database={databaseName}");
                }
                else
                {
                    newParts.Add(part);
                }
            }

            return string.Join(";", newParts);
        }

        public static bool TestConnection(string databaseName = "postgres")
        {
            try
            {
                using (var connection = CreateConnection(databaseName))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                return false;
            }
        }
    }
}