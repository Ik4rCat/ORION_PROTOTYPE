using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Backend.Models;

namespace Orion.Backend.Services
{
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }
        
        private List<TaskItem> _tasks = new List<TaskItem>();
        private string _tasksFilePath;
        
        // События для UI
        public event Action<List<TaskItem>> OnTasksUpdated;
        public event Action<TaskItem> OnTaskCreated;
        public event Action<TaskItem> OnTaskUpdated;
        // TODO: Добавить другие события (Delete и т.д.)

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _tasksFilePath = Path.Combine(Application.persistentDataPath, "tasks.json");
                LoadTasks();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async Task<(bool success, string message, TaskItem createdTask)> CreateTaskAsync(string projectId, string title, string description, string reporterUserId, TaskPriority priority = TaskPriority.Medium, string assignedUserId = null, DateTime? dueDate = null, List<string> tags = null)
        {
            var project = ProjectManager.Instance.GetProjectById(projectId);
            if (project == null)
            {
                return (false, "Проект не найден.", null);
            }

            var reporter = UserManager.Instance.GetUserById(reporterUserId);
            if (reporter == null || reporter.OrganizationId != project.OrganizationId)
            {
                return (false, "Создатель задачи не найден или не принадлежит организации проекта.", null);
            }

            if (!string.IsNullOrEmpty(assignedUserId))
            {
                var assignee = UserManager.Instance.GetUserById(assignedUserId);
                if (assignee == null || assignee.OrganizationId != project.OrganizationId)
                {
                    return (false, "Назначенный пользователь не найден или не принадлежит организации проекта.", null);
                }
            }
            
            var newTask = new TaskItem(projectId, project.OrganizationId, title, description, reporterUserId, assignedUserId, dueDate)
            {
                Priority = priority,
                Tags = tags ?? new List<string>()
            };
            
            _tasks.Add(newTask);
            await SaveTasksAsync();
            
            OnTaskCreated?.Invoke(newTask); // Уведомляем UI
            OnTasksUpdated?.Invoke(GetTasksByProject(projectId)); // Обновляем список для текущего проекта
            
            Debug.Log($"Task created: {newTask.Title} in Project: {projectId}");
            return (true, "Задача успешно создана.", newTask);
        }

        public TaskItem GetTaskById(string id)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }
        
        public List<TaskItem> GetTasksByProject(string projectId)
        {
            return _tasks.Where(t => t.ProjectId == projectId).ToList();
        }

        public List<TaskItem> GetTasksByOrganization(string organizationId)
        {
             return _tasks.Where(t => t.OrganizationId == organizationId).ToList();
        }

        public List<TaskItem> GetTasksAssignedToUser(string userId)
        {
            return _tasks.Where(t => t.AssignedUserId == userId).ToList();
        }

        public async Task<bool> UpdateTaskStatusAsync(string taskId, Orion.Backend.Models.TaskStatus newStatus)
        {
            var task = GetTaskById(taskId);
            if (task == null)
            {
                Debug.LogError($"Task with ID {taskId} not found for status update.");
                return false;
            }

            task.Status = newStatus;
            await SaveTasksAsync();
            OnTaskUpdated?.Invoke(task);
            OnTasksUpdated?.Invoke(GetTasksByProject(task.ProjectId));
            Debug.Log($"Task {task.Title} status updated to {newStatus}");
            return true;
        }
        
        public async Task<bool> UpdateTaskPriorityAsync(string taskId, TaskPriority newPriority)
        {
            var task = GetTaskById(taskId);
            if (task == null)
            {
                Debug.LogError($"Task with ID {taskId} not found for priority update.");
                return false;
            }

            task.Priority = newPriority;
            await SaveTasksAsync();
            OnTaskUpdated?.Invoke(task);
            OnTasksUpdated?.Invoke(GetTasksByProject(task.ProjectId));
            Debug.Log($"Task {task.Title} priority updated to {newPriority}");
            return true;
        }

        public async Task<bool> AssignTaskAsync(string taskId, string userIdToAssign)
        {
            var task = GetTaskById(taskId);
            if (task == null)
            {
                Debug.LogError($"Task with ID {taskId} not found for assignment.");
                return false;
            }

            if (!string.IsNullOrEmpty(userIdToAssign))
            {
                var assignee = UserManager.Instance.GetUserById(userIdToAssign);
                if (assignee == null || assignee.OrganizationId != task.OrganizationId)
                {
                     Debug.LogError($"Assignee user {userIdToAssign} not found or not in the same organization as the task.");
                     return false;
                }
                task.AssignedUserId = userIdToAssign;
            }
            else
            {
                task.AssignedUserId = null; // Снять назначение
            }

            await SaveTasksAsync();
            OnTaskUpdated?.Invoke(task);
            OnTasksUpdated?.Invoke(GetTasksByProject(task.ProjectId));
            Debug.Log($"Task {task.Title} assigned to user ID: {userIdToAssign ?? "None"}");
            return true;
        }
        
        // TODO: Добавить методы для удаления, обновления описания, тегов и т.д.

        // --- Сохранение/Загрузка --- 
        private void LoadTasks()
        {
            if (File.Exists(_tasksFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_tasksFilePath);
                    _tasks = JsonUtility.FromJson<Serialization<TaskItem>>(json)?.ToList() ?? new List<TaskItem>();
                    Debug.Log($"Loaded {_tasks.Count} tasks from {_tasksFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load tasks from {_tasksFilePath}: {ex.Message}");
                    _tasks = new List<TaskItem>();
                }
            }
            else
            {
                 _tasks = new List<TaskItem>();
                 Debug.Log("Task data file not found. Starting with empty list.");
            }
             OnTasksUpdated?.Invoke(_tasks); // Уведомляем об общем списке при старте
        }

        private async Task SaveTasksAsync()
        {
            try
            {
                string json = JsonUtility.ToJson(new Serialization<TaskItem>(_tasks), true);
                await Task.Run(() => File.WriteAllText(_tasksFilePath, json));
                Debug.Log($"Saved {_tasks.Count} tasks to {_tasksFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save tasks to {_tasksFilePath}: {ex.Message}");
            }
        }
    }
} 