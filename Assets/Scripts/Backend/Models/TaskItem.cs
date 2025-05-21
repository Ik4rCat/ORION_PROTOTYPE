using System;
using System.Collections.Generic;

namespace Orion.Backend.Models
{
    public enum TaskStatus
    {
        Backlog, 
        ToDo, 
        InProgress, 
        Review, 
        Done,
        Blocked
    }

    public enum TaskPriority
    {
        Low, 
        Medium, 
        High, 
        Urgent
    }

    [Serializable]
    public class TaskItem
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string OrganizationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public string AssignedUserId { get; set; }
        public string ReporterUserId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? DueDate { get; set; }
        public List<string> Tags { get; set; }

        public TaskItem()
        {
            Id = Guid.NewGuid().ToString();
            CreationDate = DateTime.UtcNow;
            Tags = new List<string>();
        }

        public TaskItem(string projectId, string organizationId, string title, string description, string reporterUserId, string assignedUserId = null, DateTime? dueDate = null)
        {
            Id = Guid.NewGuid().ToString();
            ProjectId = projectId;
            OrganizationId = organizationId;
            Title = title;
            Description = description;
            Status = TaskStatus.ToDo;
            Priority = TaskPriority.Medium;
            AssignedUserId = assignedUserId;
            ReporterUserId = reporterUserId;
            CreationDate = DateTime.UtcNow;
            DueDate = dueDate;
            Tags = new List<string>();
        }
    }
} 