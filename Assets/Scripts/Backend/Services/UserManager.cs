using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Backend.Models;

namespace Orion.Backend.Services
{
    public class UserManager : MonoBehaviour
    {
        public static UserManager Instance { get; private set; }
        
        // Текущий авторизованный пользователь
        public User CurrentUser { get; private set; }
        
        // Событие изменения пользователя
        public event Action<User> OnUserChanged;
        
        // События входа/выхода пользователя
        public event Action<User> OnUserLoggedIn;
        public event Action<User> OnUserLoggedOut;
        
        // Список пользователей (в реальном приложении будет загружаться из базы данных)
        private List<User> _users = new List<User>();
        
        private string _usersFilePath;
        
        public event Action<List<User>> OnUsersUpdated;
        public event Action<User> OnUserCreated;
        public event Action<User> OnUserUpdated;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _usersFilePath = Path.Combine(Application.persistentDataPath, "users.json");
                LoadUsers();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Добавление тестовых пользователей
            if (_users.Count == 0)
            {
                _users.Add(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "developer",
                    Email = "dev@example.com",
                    PasswordHash = HashPassword("password"),
                    Role = UserRole.Developer
                });
                
                _users.Add(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "user",
                    Email = "user@example.com",
                    PasswordHash = HashPassword("password"),
                    Role = UserRole.User
                });
                
                SaveUsersAsync().Wait();
            }
        }
        
        // Регистрация нового пользователя
        public async Task<(bool success, string message, User createdUser)> CreateUserAsync(string username, string email, string password, UserRole role = UserRole.User)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return (false, "Username, email, and password are required.", null);
            }

            if (_users.Any(u => u.Username == username))
            {
                return (false, "Username already exists.", null);
            }

            if (_users.Any(u => u.Email == email))
            {
                return (false, "Email already exists.", null);
            }

            var newUser = new User(username, email, password, role);
            _users.Add(newUser);
            await SaveUsersAsync();

            OnUserCreated?.Invoke(newUser);
            OnUsersUpdated?.Invoke(_users);

            Debug.Log($"User created: {newUser.Username} with role {newUser.Role}");
            return (true, "User successfully created.", newUser);
        }
        
        // Регистрация пользователя (метод-обертка для совместимости с тестовыми скриптами)
        public async Task<bool> RegisterUserAsync(string username, string email, string password, UserRole role = UserRole.Developer)
        {
            var result = await CreateUserAsync(username, email, password, role);
            return result.success;
        }
        
        // Вход в аккаунт
        public async Task<(bool success, string message)> Login(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                 return (false, "Пользователь с таким email не найден.");
            }

            if (!user.ValidatePassword(password))
            {
                 return (false, "Неверный пароль.");
            }
            
            CurrentUser = user;
            OnUserChanged?.Invoke(user);
            OnUserLoggedIn?.Invoke(user);
            Debug.Log($"User logged in: {CurrentUser.Username}");
            return (true, "Вход выполнен успешно.");
        }
        
        // Вход в систему (метод-обертка для совместимости с тестовыми скриптами)
        public async Task<bool> LoginUserAsync(string email, string password)
        {
            var result = await Login(email, password);
            return result.success;
        }
        
        // Выход из аккаунта
        public void LogOut()
        {
            var oldUser = CurrentUser;
            CurrentUser = null;
            OnUserChanged?.Invoke(null);
            OnUserLoggedOut?.Invoke(oldUser);
            Debug.Log("User logged out.");
        }
        
        // Альтернативное имя для LogOut (используется в тестовых скриптах)
        public void Logout()
        {
            LogOut();
        }
        
        // Проверка, вошел ли пользователь в систему
        public bool IsUserLoggedIn()
        {
            return CurrentUser != null;
        }
        
        // Проверка существования пользователя по email
        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            await Task.Yield();
            return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        
        // Получение списка всех пользователей
        public List<User> GetAllUsers()
        {
            return _users.ToList();
        }
        
        // Получение пользователя по ID
        public User GetUserById(string id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
        
        // Получение пользователя по имени
        public User GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }

        public User GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u => u.Email == email);
        }

        public List<User> GetUsersByOrganization(string organizationId)
        {
            return _users.Where(u => u.OrganizationId == organizationId).ToList();
        }
        
        // Хеширование пароля
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string hashOfEnteredPassword = HashPassword(enteredPassword);
            return hashOfEnteredPassword == storedHash;
        }

        // Сохранение/Загрузка пользователей
        private void LoadUsers()
        {
            if (File.Exists(_usersFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_usersFilePath);
                    _users = JsonUtility.FromJson<Serialization<User>>(json)?.ToList() ?? new List<User>();
                    Debug.Log($"Loaded {_users.Count} users from {_usersFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load users from {_usersFilePath}: {ex.Message}");
                    _users = new List<User>(); // Начинаем с чистого списка в случае ошибки
                }
            }
            else
            {
                _users = new List<User>(); // Файла нет, начинаем с пустого списка
                Debug.Log("User data file not found. Starting with empty list.");
            }
            OnUsersUpdated?.Invoke(_users);
        }

        private async Task SaveUsersAsync()
        {
            try
            {
                string json = JsonUtility.ToJson(new Serialization<User>(_users), true);
                await Task.Run(() => File.WriteAllText(_usersFilePath, json));
                Debug.Log($"Saved {_users.Count} users to {_usersFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save users to {_usersFilePath}: {ex.Message}");
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = GetUserById(user.Id);
            if (existingUser == null)
            {
                Debug.LogError($"User with ID {user.Id} not found for update.");
                return false;
            }

            _users.Remove(existingUser);
            _users.Add(user);
            await SaveUsersAsync();

            OnUserUpdated?.Invoke(user);
            OnUsersUpdated?.Invoke(_users);

            Debug.Log($"User updated: {user.Username}");
            return true;
        }
    }
} 