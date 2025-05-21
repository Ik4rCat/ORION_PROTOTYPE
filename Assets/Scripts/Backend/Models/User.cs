using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Orion.Backend.Models
{
    public enum UserRole
    {
        User,
        Admin,
        Developer,
        Manager
    }

    [Serializable]
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string OrganizationId { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public List<string> TeamIds { get; set; } = new List<string>();

        // Свойство, определяющее, имеет ли пользователь доступ к инструментам разработчика
        public bool HasDeveloperAccess => Role == UserRole.Developer || Role == UserRole.Admin;

        public User()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            TeamIds = new List<string>();
        }

        public User(string username, string email, string password, UserRole role = UserRole.User)
        {
            Id = Guid.NewGuid().ToString();
            Username = username;
            Email = email;
            PasswordHash = HashPassword(password);
            Role = role;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            TeamIds = new List<string>();
        }

        public bool ValidatePassword(string password)
        {
            return PasswordHash == HashPassword(password);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 