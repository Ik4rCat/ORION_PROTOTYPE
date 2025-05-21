using System;

namespace Orion.Backend.Models
{
    public enum ActivityType
    {
        UserCreated,
        UserUpdated,
        UserLoggedIn,
        UserAddedToOrganization,
        OrganizationCreated,
        OrganizationUpdated,
        ProjectCreated,
        ProjectUpdated,
        ProjectStatusChanged,
        UserAddedToProject,
        TaskCreated,
        TaskUpdated,
        TaskStatusChanged,
        TaskPriorityChanged,
        TaskAssigned,
        CommentAdded,
        CommentUpdated
    }

    [Serializable]
    public class ActivityLog
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public ActivityType Type { get; set; }
        public string TargetId { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string EntityType { get; set; }
        public string OrganizationId { get; set; }
        public string ProjectId { get; set; }

        public ActivityLog()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
        }

        public ActivityLog(string userId, ActivityType type, string targetId, string description, 
            string entityType = null, string organizationId = null, string projectId = null)
        {
            Id = Guid.NewGuid().ToString();
            UserId = userId;
            Type = type;
            TargetId = targetId;
            Description = description;
            Timestamp = DateTime.UtcNow;
            EntityType = entityType;
            OrganizationId = organizationId;
            ProjectId = projectId;
        }
    }
} 