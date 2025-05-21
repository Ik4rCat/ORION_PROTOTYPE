using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Backend.Models;

namespace Orion.Backend.Services
{
    public class GitIntegrationManager : MonoBehaviour
    {
        public static GitIntegrationManager Instance { get; private set; }

        private List<GitRepo> _repositories = new List<GitRepo>();
        private List<GitCommit> _commits = new List<GitCommit>();
        private string _reposFilePath;
        private string _commitsFilePath;

        public event Action<List<GitRepo>> OnRepositoriesUpdated;
        public event Action<GitRepo> OnRepositoryAdded;
        public event Action<List<GitCommit>> OnCommitsUpdated;
        public event Action<GitCommit> OnCommitAdded;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _reposFilePath = Path.Combine(Application.persistentDataPath, "git_repos.json");
                _commitsFilePath = Path.Combine(Application.persistentDataPath, "git_commits.json");
                LoadData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async Task<(bool success, string message, GitRepo createdRepo)> AddRepositoryAsync(string name, string url, string projectId)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(projectId))
            {
                return (false, "Имя, URL и ID проекта обязательны.", null);
            }

            var project = ProjectManager.Instance.GetProjectById(projectId);
            if (project == null)
            {
                return (false, "Проект не найден.", null);
            }

            if (_repositories.Any(r => r.Url == url && r.ProjectId == projectId))
            {
                return (false, "Репозиторий с таким URL уже привязан к этому проекту.", null);
            }

            var newRepo = new GitRepo(name, url, projectId);
            _repositories.Add(newRepo);
            await SaveRepositoriesAsync();

            OnRepositoryAdded?.Invoke(newRepo);
            OnRepositoriesUpdated?.Invoke(_repositories);

            // Регистрируем активность
            if (ActivityLogManager.Instance != null && UserManager.Instance.CurrentUser != null)
            {
                await ActivityLogManager.Instance.LogActivityAsync(
                    UserManager.Instance.CurrentUser.Id,
                    ActivityType.ProjectUpdated,
                    newRepo.Id,
                    $"Добавлен репозиторий {name} к проекту {project.Name}",
                    "GitRepo",
                    project.OrganizationId,
                    projectId
                );
            }

            Debug.Log($"Git repository added: {name} for project {project.Name}");
            return (true, "Репозиторий успешно добавлен.", newRepo);
        }

        public async Task<(bool success, string message, GitCommit createdCommit)> AddCommitAsync(
            string sha, string message, string authorName, string authorEmail, 
            DateTime commitDate, string branchName, string repoId, string taskItemId = null)
        {
            var repo = GetRepositoryById(repoId);
            if (repo == null)
            {
                return (false, "Репозиторий не найден.", null);
            }

            if (_commits.Any(c => c.SHA == sha && c.BranchName == branchName))
            {
                return (false, "Коммит с таким SHA уже существует в системе.", null);
            }

            if (!string.IsNullOrEmpty(taskItemId))
            {
                var task = TaskManager.Instance.GetTaskById(taskItemId);
                if (task == null)
                {
                    taskItemId = null;
                    Debug.LogWarning($"Task with ID {taskItemId} not found, commit will be added without task reference.");
                }
            }

            var newCommit = new GitCommit(sha, message, authorName, authorEmail, commitDate, branchName, taskItemId);
            _commits.Add(newCommit);
            
            // Проверяем, есть ли ветка в списке веток репозитория
            if (!repo.BranchNames.Contains(branchName))
            {
                repo.BranchNames.Add(branchName);
            }
            
            repo.LastSyncTime = DateTime.UtcNow;
            
            await SaveCommitsAsync();
            await SaveRepositoriesAsync();

            OnCommitAdded?.Invoke(newCommit);
            OnCommitsUpdated?.Invoke(_commits.Where(c => c.BranchName == branchName).ToList());

            // Регистрируем активность, если коммит связан с задачей
            if (ActivityLogManager.Instance != null && !string.IsNullOrEmpty(taskItemId))
            {
                var task = TaskManager.Instance.GetTaskById(taskItemId);
                if (task != null)
                {
                    await ActivityLogManager.Instance.LogActivityAsync(
                        authorEmail, // Используем email как идентификатор, так как пользователя может не быть в системе
                        ActivityType.TaskUpdated,
                        task.Id,
                        $"Добавлен коммит: {message.Substring(0, Math.Min(50, message.Length))}...",
                        "GitCommit",
                        task.OrganizationId,
                        task.ProjectId
                    );
                }
            }

            Debug.Log($"Git commit added: {sha.Substring(0, 7)} - {message.Substring(0, Math.Min(50, message.Length))}...");
            return (true, "Коммит успешно добавлен.", newCommit);
        }

        public GitRepo GetRepositoryById(string id)
        {
            return _repositories.FirstOrDefault(r => r.Id == id);
        }

        public List<GitRepo> GetRepositoriesByProject(string projectId)
        {
            return _repositories.Where(r => r.ProjectId == projectId).ToList();
        }

        public List<GitCommit> GetCommitsByRepository(string repoId)
        {
            var repo = GetRepositoryById(repoId);
            if (repo == null)
                return new List<GitCommit>();

            return _commits.Where(c => repo.BranchNames.Contains(c.BranchName)).ToList();
        }

        public List<GitCommit> GetCommitsByTask(string taskItemId)
        {
            return _commits.Where(c => c.TaskItemId == taskItemId).OrderByDescending(c => c.CommitDate).ToList();
        }

        public List<GitCommit> GetCommitsByBranch(string repoId, string branchName)
        {
            var repo = GetRepositoryById(repoId);
            if (repo == null || !repo.BranchNames.Contains(branchName))
                return new List<GitCommit>();

            return _commits.Where(c => c.BranchName == branchName).OrderByDescending(c => c.CommitDate).ToList();
        }

        private void LoadData()
        {
            LoadRepositories();
            LoadCommits();
        }

        private void LoadRepositories()
        {
            if (File.Exists(_reposFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_reposFilePath);
                    _repositories = JsonUtility.FromJson<Serialization<GitRepo>>(json)?.ToList() ?? new List<GitRepo>();
                    Debug.Log($"Loaded {_repositories.Count} Git repositories from {_reposFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load Git repositories from {_reposFilePath}: {ex.Message}");
                    _repositories = new List<GitRepo>();
                }
            }
            else
            {
                _repositories = new List<GitRepo>();
                Debug.Log("Git repositories file not found. Starting with empty list.");
            }
            OnRepositoriesUpdated?.Invoke(_repositories);
        }

        private void LoadCommits()
        {
            if (File.Exists(_commitsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_commitsFilePath);
                    _commits = JsonUtility.FromJson<Serialization<GitCommit>>(json)?.ToList() ?? new List<GitCommit>();
                    Debug.Log($"Loaded {_commits.Count} Git commits from {_commitsFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load Git commits from {_commitsFilePath}: {ex.Message}");
                    _commits = new List<GitCommit>();
                }
            }
            else
            {
                _commits = new List<GitCommit>();
                Debug.Log("Git commits file not found. Starting with empty list.");
            }
            OnCommitsUpdated?.Invoke(_commits);
        }

        private async Task SaveRepositoriesAsync()
        {
            try
            {
                string json = JsonUtility.ToJson(new Serialization<GitRepo>(_repositories), true);
                await Task.Run(() => File.WriteAllText(_reposFilePath, json));
                Debug.Log($"Saved {_repositories.Count} Git repositories to {_reposFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save Git repositories to {_reposFilePath}: {ex.Message}");
            }
        }

        private async Task SaveCommitsAsync()
        {
            try
            {
                string json = JsonUtility.ToJson(new Serialization<GitCommit>(_commits), true);
                await Task.Run(() => File.WriteAllText(_commitsFilePath, json));
                Debug.Log($"Saved {_commits.Count} Git commits to {_commitsFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save Git commits to {_commitsFilePath}: {ex.Message}");
            }
        }
    }
} 