using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Backend.Models;

namespace Orion.Backend.Services
{
    public class ProjectManager : MonoBehaviour
    {
        public static ProjectManager Instance { get; private set; }
        
        private List<Project> _projects = new List<Project>();
        private string _projectsFilePath;
        
        // События для UI
        public event Action<List<Project>> OnProjectsUpdated;
        public event Action<Project> OnProjectCreated;
        public event Action<Project> OnProjectUpdated;
        // TODO: Добавить другие события (Update, Delete и т.д.)

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _projectsFilePath = Path.Combine(Application.persistentDataPath, "projects.json");
                LoadProjects();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async Task<(bool success, string message, Project createdProject)> CreateProjectAsync(string organizationId, string name, string description, List<string> initialMemberIds = null, DateTime? dueDate = null)
        {
            if (string.IsNullOrEmpty(organizationId) || string.IsNullOrEmpty(name))
            {
                return (false, "Organization ID and name are required.", null);
            }

            var org = OrganizationManager.Instance.GetOrganizationById(organizationId);
            if (org == null)
            {
                return (false, "Organization not found.", null);
            }

            if (initialMemberIds != null)
            {
                foreach (var memberId in initialMemberIds)
                {
                    var member = UserManager.Instance.GetUserById(memberId);
                    if (member == null || member.OrganizationId != organizationId)
                    {
                        return (false, $"User {memberId} not found or not a member of the organization.", null);
                    }
                }
            }

            var newProject = new Project(organizationId, name, description, dueDate);
            if (initialMemberIds != null)
            {
                newProject.MemberUserIds.AddRange(initialMemberIds);
            }

            _projects.Add(newProject);
            await SaveProjectsAsync();

            OnProjectCreated?.Invoke(newProject);
            OnProjectsUpdated?.Invoke(_projects);

            Debug.Log($"Project created: {newProject.Name} in Organization: {organizationId}");
            return (true, "Project successfully created.", newProject);
        }

        public Project GetProjectById(string id)
        {
            return _projects.FirstOrDefault(p => p.Id == id);
        }
        
        public List<Project> GetProjectsByOrganization(string organizationId)
        {
            return _projects.Where(p => p.OrganizationId == organizationId).ToList();
        }

        public async Task<bool> AddUserToProjectAsync(string projectId, string userId)
        {
            var project = GetProjectById(projectId);
            if (project == null)
            {
                Debug.LogError($"Project with ID {projectId} not found.");
                return false;
            }

            var user = UserManager.Instance.GetUserById(userId);
            if (user == null)
            {
                Debug.LogError($"User with ID {userId} not found.");
                return false;
            }

            if (user.OrganizationId != project.OrganizationId)
            {
                Debug.LogError($"User {user.Username} does not belong to the project's organization.");
                return false;
            }

            if (project.MemberUserIds.Contains(userId))
            {
                Debug.LogError($"User {user.Username} is already a member of this project.");
                return false;
            }

            project.MemberUserIds.Add(userId);
            await SaveProjectsAsync();

            OnProjectUpdated?.Invoke(project);
            OnProjectsUpdated?.Invoke(_projects);

            Debug.Log($"User {user.Username} added to project {project.Name}");
            return true;
        }
        
        // TODO: Добавить методы для удаления, обновления статуса, изменения участников и т.д.

        // --- Сохранение/Загрузка --- 
        private void LoadProjects()
        {
            if (File.Exists(_projectsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_projectsFilePath);
                    _projects = JsonUtility.FromJson<Serialization<Project>>(json)?.ToList() ?? new List<Project>();
                    Debug.Log($"Loaded {_projects.Count} projects from {_projectsFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load projects from {_projectsFilePath}: {ex.Message}");
                    _projects = new List<Project>();
                }
            }
            else
            {
                 _projects = new List<Project>();
                 Debug.Log("Project data file not found. Starting with empty list.");
            }
             OnProjectsUpdated?.Invoke(_projects); // Уведомляем об общем списке при старте
        }

        private async Task SaveProjectsAsync()
        {
            try
            {
                string json = JsonUtility.ToJson(new Serialization<Project>(_projects), true);
                await Task.Run(() => File.WriteAllText(_projectsFilePath, json));
                Debug.Log($"Saved {_projects.Count} projects to {_projectsFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save projects to {_projectsFilePath}: {ex.Message}");
            }
        }
    }
} 