using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Backend.Models;

namespace Orion.Backend.Services
{
    public class CommentManager : MonoBehaviour
    {
        public static CommentManager Instance { get; private set; }

        private List<Comment> _comments = new List<Comment>();
        private string _commentsFilePath;

        public event Action<List<Comment>> OnCommentsUpdated;
        public event Action<Comment> OnCommentCreated;
        public event Action<Comment> OnCommentUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _commentsFilePath = Path.Combine(Application.persistentDataPath, "comments.json");
                LoadComments();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async Task<(bool success, string message, Comment createdComment)> AddCommentAsync(string taskItemId, string authorId, string text, string parentCommentId = null)
        {
            var task = TaskManager.Instance.GetTaskById(taskItemId);
            if (task == null)
            {
                return (false, "Задача не найдена.", null);
            }

            var author = UserManager.Instance.GetUserById(authorId);
            if (author == null)
            {
                return (false, "Автор комментария не найден.", null);
            }

            if (string.IsNullOrEmpty(text))
            {
                return (false, "Текст комментария не может быть пустым.", null);
            }

            if (!string.IsNullOrEmpty(parentCommentId))
            {
                var parentComment = _comments.FirstOrDefault(c => c.Id == parentCommentId);
                if (parentComment == null)
                {
                    return (false, "Родительский комментарий не найден.", null);
                }
            }

            var newComment = new Comment(taskItemId, authorId, text, parentCommentId);
            _comments.Add(newComment);
            await SaveCommentsAsync();

            OnCommentCreated?.Invoke(newComment);
            OnCommentsUpdated?.Invoke(GetCommentsByTaskId(taskItemId));

            // Регистрируем активность
            if (ActivityLogManager.Instance != null)
            {
                await ActivityLogManager.Instance.LogActivityAsync(
                    authorId, 
                    ActivityType.CommentAdded,
                    newComment.Id,
                    $"Добавлен комментарий к задаче {task.Title}",
                    "Comment",
                    task.OrganizationId,
                    task.ProjectId
                );
            }

            Debug.Log($"Comment added to task {task.Title} by {author.Username}");
            return (true, "Комментарий успешно добавлен.", newComment);
        }

        public async Task<bool> UpdateCommentAsync(string commentId, string authorId, string newText)
        {
            var comment = _comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null)
            {
                Debug.LogError($"Comment with ID {commentId} not found for update.");
                return false;
            }

            if (comment.AuthorId != authorId)
            {
                Debug.LogError($"User {authorId} cannot update comment authored by {comment.AuthorId}.");
                return false;
            }

            if (string.IsNullOrEmpty(newText))
            {
                Debug.LogError("Comment text cannot be empty.");
                return false;
            }

            comment.Text = newText;
            comment.UpdatedAt = DateTime.UtcNow;
            await SaveCommentsAsync();

            OnCommentUpdated?.Invoke(comment);
            OnCommentsUpdated?.Invoke(GetCommentsByTaskId(comment.TaskItemId));

            // Регистрируем активность
            if (ActivityLogManager.Instance != null)
            {
                var task = TaskManager.Instance.GetTaskById(comment.TaskItemId);
                if (task != null)
                {
                    await ActivityLogManager.Instance.LogActivityAsync(
                        authorId,
                        ActivityType.CommentUpdated,
                        comment.Id,
                        $"Обновлен комментарий к задаче {task.Title}",
                        "Comment",
                        task.OrganizationId,
                        task.ProjectId
                    );
                }
            }

            Debug.Log($"Comment {commentId} updated");
            return true;
        }

        public List<Comment> GetCommentsByTaskId(string taskItemId)
        {
            return _comments.Where(c => c.TaskItemId == taskItemId).ToList();
        }

        public List<Comment> GetCommentsByAuthor(string authorId)
        {
            return _comments.Where(c => c.AuthorId == authorId).ToList();
        }

        public List<Comment> GetCommentReplies(string parentCommentId)
        {
            return _comments.Where(c => c.ParentCommentId == parentCommentId).ToList();
        }

        private void LoadComments()
        {
            if (File.Exists(_commentsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_commentsFilePath);
                    _comments = JsonUtility.FromJson<Serialization<Comment>>(json)?.ToList() ?? new List<Comment>();
                    Debug.Log($"Loaded {_comments.Count} comments from {_commentsFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load comments from {_commentsFilePath}: {ex.Message}");
                    _comments = new List<Comment>();
                }
            }
            else
            {
                _comments = new List<Comment>();
                Debug.Log("Comments data file not found. Starting with empty list.");
            }
            OnCommentsUpdated?.Invoke(_comments);
        }

        private async Task SaveCommentsAsync()
        {
            try
            {
                string json = JsonUtility.ToJson(new Serialization<Comment>(_comments), true);
                await Task.Run(() => File.WriteAllText(_commentsFilePath, json));
                Debug.Log($"Saved {_comments.Count} comments to {_commentsFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save comments to {_commentsFilePath}: {ex.Message}");
            }
        }
    }
} 