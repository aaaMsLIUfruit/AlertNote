using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StickyAlerts.DataAccess;
using System;
using StickyAlerts.Services;
using System.Security.Cryptography;
using System.Text;

namespace StickyAlerts.Services
{
    public interface IAuthService
    {
        bool Login(string username, string password);
        bool Register(string username, string password);
        Guid? CurrentUserId { get; } 

        void ResetLocalData();
    }

    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IAlertService _alertService;

        private Guid? _currentUserId; 

        public AuthService(ILogger<AuthService> logger, IAlertService alertService)
        {
            _logger = logger;
            _alertService = alertService;
        }
        public Guid? CurrentUserId => _currentUserId;

        public bool Login(string username, string password)
        {
            try
            {
                _logger.LogInformation("Attempting login for user: {username}", username);

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT PasswordHash, Salt FROM Users WHERE Username = $username";
                    cmd.Parameters.AddWithValue("$username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            _logger.LogWarning("Login failed - user not found: {username}", username);
                            return false;
                        }
                        var userId = reader.GetGuid(0);
                        var salt = reader["Salt"].ToString();
                        var storedHash = reader["PasswordHash"].ToString();
                        var inputHash = HashPassword(password, salt);

                        bool success = storedHash == inputHash;
                        if (success)
                        {
                            _currentUserId = userId;
                            _alertService.SetCurrentUser(userId);
                            _logger.LogInformation("Login successful for user: {username}", username);
                        }
                        else
                        {
                            _logger.LogWarning("Login failed - invalid password for user: {username}", username);
                        }

                        return success;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {username}", username);
                throw;
            }
        }

        public bool Register(string username, string password)
        {
            try
            {
                _logger.LogInformation("Attempting to register user: {username}", username);

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    var checkCmd = connection.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = $username";
                    checkCmd.Parameters.AddWithValue("$username", username);
                    if ((long)checkCmd.ExecuteScalar() > 0)
                    {
                        _logger.LogWarning("Registration failed - username already exists: {username}", username);
                        return false;
                    }

                    // Create user
                    var salt = GenerateSalt();
                    var hash = HashPassword(password, salt);

                    var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = @"
                        INSERT INTO Users (Username, PasswordHash, Salt)
                        VALUES ($username, $hash, $salt)
                        SELECT last_insert_rowid();";
                    insertCmd.Parameters.AddWithValue("$username", username);
                    insertCmd.Parameters.AddWithValue("$hash", hash);
                    insertCmd.Parameters.AddWithValue("$salt", salt);

                    var userId = Convert.ToInt64(insertCmd.ExecuteScalar());
                    _currentUserId = Guid.NewGuid();
                

                    bool success = insertCmd.ExecuteNonQuery() > 0;
                    if (success)
                    {
                        _logger.LogInformation("Registration successful for user: {username}", username);
                    }
                    else
                    {
                        _logger.LogError("Registration failed - database error for user: {username}", username);
                    }

                    return success;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {username}", username);
                throw;
            }
        }

        public void ResetLocalData()
        {
            try
            {
                _logger.LogInformation("Resetting all local data");
                _currentUserId = null;

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();

                    cmd.CommandText = @"
                        DELETE FROM Notes;
                        DELETE FROM Tags;
                        DELETE FROM NoteTags;
                        DELETE FROM Reminders;
                        DELETE FROM Users;";

                    cmd.ExecuteNonQuery();
                    _logger.LogInformation("Local data reset completed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during local data reset");
                throw;
            }
        }

        private static string GenerateSalt()
        {
            var bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(bytes);
        }
    }
}