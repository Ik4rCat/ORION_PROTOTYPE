using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Backend.Models;

namespace Orion.Backend.Services
{
    public class ActivityLogManager : MonoBehaviour
    {
        public static ActivityLogManager Instance { get; private set; }

        private List<ActivityLog> _logs = new List<ActivityLog>();
        private string _logsFilePath;
        private int _maxLogCount = 10000; // Лимит для хранения логов
        private bool _isLoggingEnabled = true;

        public event Action<List<ActivityLog>> OnLogsUpdated;
        public event Action<ActivityLog> OnLogAdded;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _logsFilePath = Path.Combine(Application.persistentDataPath, "activity_logs.json");
                LoadLogs();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async Task<ActivityLog> LogActivityAsync(string userId, ActivityType type, string targetId, 
            string description, string entityType = null, string organizationId = null, string projectId = null)
        {
            if (!_isLoggingEnabled)
                return null;

            var log = new ActivityLog(userId, type, targetId, description, entityType, organizationId, projectId);
            
            _logs.Add(log);
            
            // Если превысили лимит, удалим самые старые логи
            if (_logs.Count > _maxLogCount)
            {
                int countToRemove = _logs.Count - _maxLogCount;
                _logs = _logs.OrderByDescending(l => l.Timestamp).Take(_maxLogCount).ToList();
                Debug.Log($"Removed {countToRemove} old logs due to limit");
            }
            
            await SaveLogsAsync();
            
            OnLogAdded?.Invoke(log);
            OnLogsUpdated?.Invoke(GetRecentLogs(100)); // Обновляем последние 100 логов для UI
            
            return log;
        }

        public List<ActivityLog> GetRecentLogs(int count)
        {
            return _logs.OrderByDescending(l => l.Timestamp).Take(count).ToList();
        }

        public List<ActivityLog> GetLogsByUser(string userId, int count = 100)
        {
            return _logs.Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToList();
        }

        public List<ActivityLog> GetLogsByOrganization(string organizationId, int count = 100)
        {
            return _logs.Where(l => l.OrganizationId == organizationId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToList();
        }

        public List<ActivityLog> GetLogsByProject(string projectId, int count = 100)
        {
            return _logs.Where(l => l.ProjectId == projectId)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToList();
        }

        public List<ActivityLog> GetLogsByEntityType(string entityType, int count = 100)
        {
            return _logs.Where(l => l.EntityType == entityType)
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToList();
        }

        public void SetLoggingEnabled(bool enabled)
        {
            _isLoggingEnabled = enabled;
            Debug.Log($"Activity logging is now {(enabled ? "enabled" : "disabled")}");
        }

        private void LoadLogs()
        {
            if (File.Exists(_logsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_logsFilePath);
                    _logs = JsonUtility.FromJson<Serialization<ActivityLog>>(json)?.ToList() ?? new List<ActivityLog>();
                    Debug.Log($"Loaded {_logs.Count} activity logs from {_logsFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load activity logs from {_logsFilePath}: {ex.Message}");
                    _logs = new List<ActivityLog>();
                }
            }
            else
            {
                _logs = new List<ActivityLog>();
                Debug.Log("Activity logs file not found. Starting with empty list.");
            }
            OnLogsUpdated?.Invoke(GetRecentLogs(100));
        }

        private async Task SaveLogsAsync()
        {
            try
            {
                string json = JsonUtility.ToJson(new Serialization<ActivityLog>(_logs), true);
                await Task.Run(() => File.WriteAllText(_logsFilePath, json));
                Debug.Log($"Saved {_logs.Count} activity logs to {_logsFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save activity logs to {_logsFilePath}: {ex.Message}");
            }
        }
    }
} 