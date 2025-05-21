using System;

namespace Orion.Backend.Models
{
    [Serializable]
    public class Comment
    {
        public string Id { get; set; }
        public string TaskItemId { get; set; }
        public string AuthorId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string ParentCommentId { get; set; }

        public Comment()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }

        public Comment(string taskItemId, string authorId, string text, string parentCommentId = null)
        {
            Id = Guid.NewGuid().ToString();
            TaskItemId = taskItemId;
            AuthorId = authorId;
            Text = text;
            CreatedAt = DateTime.UtcNow;
            ParentCommentId = parentCommentId;
        }
    }
} 