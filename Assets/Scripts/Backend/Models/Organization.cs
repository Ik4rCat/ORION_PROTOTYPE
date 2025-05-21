using System;
using System.Collections.Generic;

namespace Orion.Backend.Models
{
    [Serializable]
    public class Organization
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OwnerUserId { get; set; }
        public List<string> MemberUserIds { get; set; }

        public Organization()
        {
            Id = Guid.NewGuid().ToString();
            MemberUserIds = new List<string>();
        }

        public Organization(string name, string description, string ownerUserId)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Description = description;
            OwnerUserId = ownerUserId;
            MemberUserIds = new List<string> { ownerUserId };
        }
    }
} 