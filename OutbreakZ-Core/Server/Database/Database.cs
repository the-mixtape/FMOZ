using System;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using DbUp;
using Npgsql;

namespace OutbreakZCore.Server.Database
{
    public static class Database
    {
        private static string _connectionString;

        public static void Initialize(string connectionString, string migrationsFolder)
        {
            _connectionString = connectionString;
            ApplyMigrations(migrationsFolder);
        }

        private static void ApplyMigrations(string migrationsFolder)
        {
            var upgrader = DeployChanges.To
                .PostgresqlDatabase(_connectionString)
                .WithScriptsFromFileSystem(migrationsFolder)
                .LogToConsole()
                .Build();

            if (!upgrader.IsUpgradeRequired())
            {
                Debug.WriteLine("No migrations to apply.");
                return;
            }

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Debug.WriteLine("Migration failed:");
                Debug.WriteLine(result.Error.ToString());
                throw result.Error;
            }

            Console.WriteLine("Migrations applied successfully!");
        }

        public static async Task<(int? Id, bool WasCreated)> GetOrCreateUser(string license)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    {
                        const string query = "SELECT id FROM users WHERE license = @license LIMIT 1";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@license", license);
                            var result = await command.ExecuteScalarAsync();
                            if (result != null)
                            {
                                var userId = Convert.ToInt32(result);
                                return (userId, false);
                            }
                        }
                    }

                    {
                        
                        const string query = "INSERT INTO users (license) VALUES (@license) RETURNING id";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@license", license);
                            var result = await command.ExecuteScalarAsync();
                            var newUserId = Convert.ToInt32(result);
                            return (newUserId, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking user existence: {ex.Message}");
                    return (null, false);
                }
            }
        }

        // public static async Task<int?> CreateNewUser(string license)
        // {
        //     
        // }
    }
}