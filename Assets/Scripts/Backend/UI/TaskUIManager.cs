using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Orion.Backend.Models;
using Orion.Backend.Services;

namespace Orion.Backend.UI
{
    public class TaskUIManager : MonoBehaviour
    {
        [Header("Task Board")]
        [SerializeField] private GameObject taskBoardPanel;
        [SerializeField] private Transform backlogColumn;
        [SerializeField] private Transform todoColumn;
        [SerializeField] private Transform inProgressColumn;
        [SerializeField] private Transform reviewColumn;
        [SerializeField] private Transform doneColumn;
        [SerializeField] private Transform blockedColumn;

        [Header("Task Creation")]
        [SerializeField] private GameObject createTaskPanel;
        [SerializeField] private TMP_InputField taskTitleInput;
        [SerializeField] private TMP_InputField taskDescriptionInput;
        [SerializeField] private TMP_Dropdown priorityDropdown;
        [SerializeField] private TMP_Dropdown assigneeDropdown;
        [SerializeField] private TMP_InputField dueDateInput;
        [SerializeField] private TMP_InputField tagsInput;
        [SerializeField] private Button createTaskButton;
        [SerializeField] private Button cancelTaskCreationButton;

        [Header("Task Details")]
        [SerializeField] private GameObject taskDetailsPanel;
        [SerializeField] private TextMeshProUGUI taskTitleText;
        [SerializeField] private TextMeshProUGUI taskDescriptionText;
        [SerializeField] private TextMeshProUGUI taskStatusText;
        [SerializeField] private TextMeshProUGUI taskPriorityText;
        [SerializeField] private TextMeshProUGUI taskAssigneeText;
        [SerializeField] private TextMeshProUGUI taskReporterText;
        [SerializeField] private TextMeshProUGUI taskCreationDateText;
        [SerializeField] private TextMeshProUGUI taskDueDateText;
        [SerializeField] private Transform taskTagsContainer;
        [SerializeField] private Button closeTaskDetailsButton;
        [SerializeField] private Button editTaskButton;

        [Header("Comments")]
        [SerializeField] private Transform commentsContainer;
        [SerializeField] private TMP_InputField newCommentInput;
        [SerializeField] private Button addCommentButton;

        [Header("Git Integration")]
        [SerializeField] private Transform commitsContainer;

        [Header("Task Item Prefab")]
        [SerializeField] private GameObject taskItemPrefab;
        [SerializeField] private GameObject tagPrefab;
        [SerializeField] private GameObject commentPrefab;
        [SerializeField] private GameObject commitPrefab;

        private Project _currentProject;
        private TaskItem _selectedTask;
        private Dictionary<string, GameObject> _taskCards = new Dictionary<string, GameObject>();

        private void Start()
        {
            // Инициализация UI элементов
            InitializeUI();

            // Подписка на события
            SubscribeToEvents();
        }

        private void InitializeUI()
        {
            priorityDropdown.ClearOptions();
            priorityDropdown.AddOptions(new List<string> {
                "Низкий", "Средний", "Высокий", "Срочный"
            });

            assigneeDropdown.ClearOptions();
            assigneeDropdown.AddOptions(new List<string> { "Не назначен" });

            createTaskButton.onClick.AddListener(OnCreateTaskButtonClicked);
            cancelTaskCreationButton.onClick.AddListener(() => ShowTaskBoard());
            closeTaskDetailsButton.onClick.AddListener(() => ShowTaskBoard());
            editTaskButton.onClick.AddListener(OnEditTaskButtonClicked);
            addCommentButton.onClick.AddListener(OnAddCommentButtonClicked);

            taskBoardPanel.SetActive(true);
            createTaskPanel.SetActive(false);
            taskDetailsPanel.SetActive(false);
        }

        private void SubscribeToEvents()
        {
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTasksUpdated += OnTasksUpdated;
                TaskManager.Instance.OnTaskCreated += OnTaskCreated;
                TaskManager.Instance.OnTaskUpdated += OnTaskUpdated;
            }

            if (CommentManager.Instance != null)
            {
                CommentManager.Instance.OnCommentsUpdated += OnCommentsUpdated;
                CommentManager.Instance.OnCommentCreated += OnCommentCreated;
            }

            if (GitIntegrationManager.Instance != null)
            {
                GitIntegrationManager.Instance.OnCommitsUpdated += OnCommitsUpdated;
            }
        }

        public void SetCurrentProject(Project project)
        {
            _currentProject = project;
            LoadProjectTasks();
            UpdateAssigneeDropdown();
        }

        private void LoadProjectTasks()
        {
            if (_currentProject == null || TaskManager.Instance == null)
                return;

            ClearTaskColumns();
            var tasks = TaskManager.Instance.GetTasksByProject(_currentProject.Id);
            
            foreach (var task in tasks)
            {
                CreateTaskCard(task);
            }
        }

        private void ClearTaskColumns()
        {
            foreach (var taskCard in _taskCards.Values)
            {
                Destroy(taskCard);
            }
            _taskCards.Clear();
        }

        private void CreateTaskCard(TaskItem task)
        {
            if (_taskCards.ContainsKey(task.Id))
            {
                UpdateTaskCard(task);
                return;
            }

            GameObject taskCard = Instantiate(taskItemPrefab, GetColumnForStatus(task.Status));
            
            // Настройка карточки задачи
            taskCard.name = $"Task_{task.Id}";
            
            TextMeshProUGUI titleText = taskCard.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null) titleText.text = task.Title;
            
            TextMeshProUGUI priorityText = taskCard.transform.Find("PriorityText")?.GetComponent<TextMeshProUGUI>();
            if (priorityText != null) priorityText.text = GetPriorityText(task.Priority);
            
            Button detailsButton = taskCard.transform.Find("DetailsButton")?.GetComponent<Button>();
            if (detailsButton != null) detailsButton.onClick.AddListener(() => ShowTaskDetails(task));
            
            _taskCards[task.Id] = taskCard;
        }

        private void UpdateTaskCard(TaskItem task)
        {
            if (!_taskCards.TryGetValue(task.Id, out GameObject taskCard))
                return;

            // Обновление карточки задачи
            TextMeshProUGUI titleText = taskCard.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null) titleText.text = task.Title;
            
            TextMeshProUGUI priorityText = taskCard.transform.Find("PriorityText")?.GetComponent<TextMeshProUGUI>();
            if (priorityText != null) priorityText.text = GetPriorityText(task.Priority);
            
            // Если изменился статус, переместить в нужную колонку
            if (taskCard.transform.parent != GetColumnForStatus(task.Status))
            {
                taskCard.transform.SetParent(GetColumnForStatus(task.Status));
            }
        }

        private Transform GetColumnForStatus(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Backlog: return backlogColumn;
                case TaskStatus.ToDo: return todoColumn;
                case TaskStatus.InProgress: return inProgressColumn;
                case TaskStatus.Review: return reviewColumn;
                case TaskStatus.Done: return doneColumn;
                case TaskStatus.Blocked: return blockedColumn;
                default: return todoColumn;
            }
        }

        private void UpdateAssigneeDropdown()
        {
            if (_currentProject == null || UserManager.Instance == null)
                return;

            assigneeDropdown.ClearOptions();
            var options = new List<string> { "Не назначен" };
            
            foreach (var userId in _currentProject.MemberUserIds)
            {
                var user = UserManager.Instance.GetUserById(userId);
                if (user != null)
                {
                    options.Add(user.Username);
                }
            }
            
            assigneeDropdown.AddOptions(options);
        }

        private void ShowTaskBoard()
        {
            taskBoardPanel.SetActive(true);
            createTaskPanel.SetActive(false);
            taskDetailsPanel.SetActive(false);
        }

        private void ShowCreateTaskPanel()
        {
            taskBoardPanel.SetActive(false);
            createTaskPanel.SetActive(true);
            taskDetailsPanel.SetActive(false);
            
            // Очистка полей формы
            taskTitleInput.text = "";
            taskDescriptionInput.text = "";
            priorityDropdown.value = 1; // Средний приоритет по умолчанию
            assigneeDropdown.value = 0; // Не назначен по умолчанию
            dueDateInput.text = "";
            tagsInput.text = "";
        }

        private void ShowTaskDetails(TaskItem task)
        {
            _selectedTask = task;
            
            taskBoardPanel.SetActive(false);
            createTaskPanel.SetActive(false);
            taskDetailsPanel.SetActive(true);
            
            // Заполнение данных задачи
            taskTitleText.text = task.Title;
            taskDescriptionText.text = task.Description;
            taskStatusText.text = GetStatusText(task.Status);
            taskPriorityText.text = GetPriorityText(task.Priority);
            
            var assignee = UserManager.Instance.GetUserById(task.AssignedUserId);
            taskAssigneeText.text = assignee != null ? assignee.Username : "Не назначен";
            
            var reporter = UserManager.Instance.GetUserById(task.ReporterUserId);
            taskReporterText.text = reporter != null ? reporter.Username : "Неизвестно";
            
            taskCreationDateText.text = task.CreationDate.ToString("dd.MM.yyyy HH:mm");
            taskDueDateText.text = task.DueDate.HasValue ? task.DueDate.Value.ToString("dd.MM.yyyy") : "Не задан";
            
            // Очистка и отображение тегов
            foreach (Transform child in taskTagsContainer)
            {
                Destroy(child.gameObject);
            }
            
            if (task.Tags != null && task.Tags.Count > 0)
            {
                foreach (var tag in task.Tags)
                {
                    GameObject tagObj = Instantiate(tagPrefab, taskTagsContainer);
                    TextMeshProUGUI tagText = tagObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (tagText != null) tagText.text = tag;
                }
            }
            
            // Загрузка комментариев
            LoadComments();
            
            // Загрузка коммитов
            LoadCommits();
        }

        private void LoadComments()
        {
            if (_selectedTask == null || CommentManager.Instance == null)
                return;

            foreach (Transform child in commentsContainer)
            {
                Destroy(child.gameObject);
            }

            var comments = CommentManager.Instance.GetCommentsByTaskId(_selectedTask.Id);
            
            foreach (var comment in comments.OrderByDescending(c => c.CreatedAt))
            {
                CreateCommentUI(comment);
            }
        }

        private void CreateCommentUI(Comment comment)
        {
            GameObject commentObj = Instantiate(commentPrefab, commentsContainer);
            
            TextMeshProUGUI authorText = commentObj.transform.Find("AuthorText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = commentObj.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI contentText = commentObj.transform.Find("ContentText")?.GetComponent<TextMeshProUGUI>();
            
            var author = UserManager.Instance.GetUserById(comment.AuthorId);
            
            if (authorText != null) authorText.text = author != null ? author.Username : "Неизвестно";
            if (dateText != null) dateText.text = comment.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            if (contentText != null) contentText.text = comment.Text;
        }

        private void LoadCommits()
        {
            if (_selectedTask == null || GitIntegrationManager.Instance == null)
                return;

            foreach (Transform child in commitsContainer)
            {
                Destroy(child.gameObject);
            }

            var commits = GitIntegrationManager.Instance.GetCommitsByTask(_selectedTask.Id);
            
            foreach (var commit in commits)
            {
                CreateCommitUI(commit);
            }
        }

        private void CreateCommitUI(GitCommit commit)
        {
            GameObject commitObj = Instantiate(commitPrefab, commitsContainer);
            
            TextMeshProUGUI shaText = commitObj.transform.Find("SHAText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI authorText = commitObj.transform.Find("AuthorText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = commitObj.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI messageText = commitObj.transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI branchText = commitObj.transform.Find("BranchText")?.GetComponent<TextMeshProUGUI>();
            
            if (shaText != null) shaText.text = commit.SHA.Substring(0, 7);
            if (authorText != null) authorText.text = commit.AuthorName;
            if (dateText != null) dateText.text = commit.CommitDate.ToString("dd.MM.yyyy HH:mm");
            if (messageText != null) messageText.text = commit.Message;
            if (branchText != null) branchText.text = commit.BranchName;
        }

        private async void OnCreateTaskButtonClicked()
        {
            if (_currentProject == null || TaskManager.Instance == null || UserManager.Instance.CurrentUser == null)
                return;

            string title = taskTitleInput.text.Trim();
            string description = taskDescriptionInput.text.Trim();
            
            if (string.IsNullOrEmpty(title))
            {
                Debug.LogError("Task title is required");
                return;
            }
            
            TaskPriority priority = GetSelectedPriority();
            string assigneeId = GetSelectedAssigneeId();
            DateTime? dueDate = ParseDueDate();
            List<string> tags = ParseTags();
            
            var result = await TaskManager.Instance.CreateTaskAsync(
                _currentProject.Id,
                title,
                description,
                UserManager.Instance.CurrentUser.Id,
                priority,
                assigneeId,
                dueDate,
                tags
            );
            
            if (result.success)
            {
                ShowTaskBoard();
            }
            else
            {
                Debug.LogError($"Failed to create task: {result.message}");
            }
        }

        private void OnEditTaskButtonClicked()
        {
            // Реализация редактирования задачи
            Debug.Log("Edit task clicked - functionality to be implemented");
        }

        private async void OnAddCommentButtonClicked()
        {
            if (_selectedTask == null || CommentManager.Instance == null || UserManager.Instance.CurrentUser == null)
                return;

            string commentText = newCommentInput.text.Trim();
            
            if (string.IsNullOrEmpty(commentText))
            {
                Debug.LogError("Comment text is required");
                return;
            }
            
            var result = await CommentManager.Instance.AddCommentAsync(
                _selectedTask.Id,
                UserManager.Instance.CurrentUser.Id,
                commentText
            );
            
            if (result.success)
            {
                newCommentInput.text = "";
                // Комментарии будут загружены автоматически через событие OnCommentCreated
            }
            else
            {
                Debug.LogError($"Failed to add comment: {result.message}");
            }
        }

        private TaskPriority GetSelectedPriority()
        {
            switch (priorityDropdown.value)
            {
                case 0: return TaskPriority.Low;
                case 1: return TaskPriority.Medium;
                case 2: return TaskPriority.High;
                case 3: return TaskPriority.Urgent;
                default: return TaskPriority.Medium;
            }
        }

        private string GetSelectedAssigneeId()
        {
            if (assigneeDropdown.value == 0) // "Не назначен"
                return null;
            
            if (_currentProject == null || assigneeDropdown.value - 1 >= _currentProject.MemberUserIds.Count)
                return null;
            
            return _currentProject.MemberUserIds[assigneeDropdown.value - 1];
        }

        private DateTime? ParseDueDate()
        {
            if (string.IsNullOrEmpty(dueDateInput.text))
                return null;
            
            if (DateTime.TryParse(dueDateInput.text, out DateTime result))
                return result;
            
            return null;
        }

        private List<string> ParseTags()
        {
            if (string.IsNullOrEmpty(tagsInput.text))
                return new List<string>();
            
            return tagsInput.text.Split(',', ';')
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrEmpty(tag))
                .ToList();
        }

        private string GetPriorityText(TaskPriority priority)
        {
            switch (priority)
            {
                case TaskPriority.Low: return "Низкий";
                case TaskPriority.Medium: return "Средний";
                case TaskPriority.High: return "Высокий";
                case TaskPriority.Urgent: return "Срочный";
                default: return "Неизвестно";
            }
        }

        private string GetStatusText(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Backlog: return "Бэклог";
                case TaskStatus.ToDo: return "К выполнению";
                case TaskStatus.InProgress: return "В работе";
                case TaskStatus.Review: return "На проверке";
                case TaskStatus.Done: return "Выполнено";
                case TaskStatus.Blocked: return "Заблокировано";
                default: return "Неизвестно";
            }
        }

        private void OnTasksUpdated(List<TaskItem> tasks)
        {
            LoadProjectTasks();
        }

        private void OnTaskCreated(TaskItem task)
        {
            if (task.ProjectId == _currentProject?.Id)
            {
                CreateTaskCard(task);
            }
        }

        private void OnTaskUpdated(TaskItem task)
        {
            if (task.ProjectId == _currentProject?.Id)
            {
                UpdateTaskCard(task);
            }
            
            if (_selectedTask?.Id == task.Id)
            {
                ShowTaskDetails(task);
            }
        }

        private void OnCommentsUpdated(List<Comment> comments)
        {
            if (_selectedTask != null)
            {
                LoadComments();
            }
        }

        private void OnCommentCreated(Comment comment)
        {
            if (_selectedTask?.Id == comment.TaskItemId)
            {
                CreateCommentUI(comment);
            }
        }

        private void OnCommitsUpdated(List<GitCommit> commits)
        {
            if (_selectedTask != null)
            {
                LoadCommits();
            }
        }

        private void OnDestroy()
        {
            // Отписка от событий
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.OnTasksUpdated -= OnTasksUpdated;
                TaskManager.Instance.OnTaskCreated -= OnTaskCreated;
                TaskManager.Instance.OnTaskUpdated -= OnTaskUpdated;
            }

            if (CommentManager.Instance != null)
            {
                CommentManager.Instance.OnCommentsUpdated -= OnCommentsUpdated;
                CommentManager.Instance.OnCommentCreated -= OnCommentCreated;
            }

            if (GitIntegrationManager.Instance != null)
            {
                GitIntegrationManager.Instance.OnCommitsUpdated -= OnCommitsUpdated;
            }
        }
    }
} 