using System;
using System.Text.Json.Serialization;

namespace StickyAlerts.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreatedTime { get; set; }

        public User()
        {
            CreatedTime = DateTime.Now;
        }

        public User(string username, string password)
        {
            Username = username;
            Password = password;
            CreatedTime = DateTime.Now;
        }
    }
} 