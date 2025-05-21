using System;
using System.Collections.Generic;

namespace Orion.Backend.Models
{
    public enum ProjectStatus
    {
        Active,
        OnHold,
        Completed,
        Cancelled
    }

    [Serializable]
    public class Project
    {
        public string Id { get; set; }
        public string OrganizationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? DueDate { get; set; }
        public List<string> MemberUserIds { get; set; }

        public Project()
        {
            Id = Guid.NewGuid().ToString();
            CreationDate = DateTime.UtcNow;
            MemberUserIds = new List<string>();
        }

        public Project(string organizationId, string name, string description, DateTime? dueDate = null)
        {
            Id = Guid.NewGuid().ToString();
            OrganizationId = organizationId;
            Name = name;
            Description = description;
            Status = ProjectStatus.Active;
            CreationDate = DateTime.UtcNow;
            DueDate = dueDate;
            MemberUserIds = new List<string>();
        }
    }
} 