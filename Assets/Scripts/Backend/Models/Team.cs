using System;
using System.Collections.Generic;

namespace Orion.Models
{
    [Serializable]
    public class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public List<TeamMember> Members { get; set; } = new List<TeamMember>();
        public List<string> BoardIds { get; set; } = new List<string>();
        public List<string> CanvasIds { get; set; } = new List<string>();
        public List<string> NoteIds { get; set; } = new List<string>();
        
        // Методы управления командой
        public bool AddMember(string userId, TeamRole role)
        {
            if (Members.Exists(m => m.UserId == userId))
                return false;
                
            Members.Add(new TeamMember 
            { 
                UserId = userId,
                Role = role
            });
            
            return true;
        }
        
        public bool RemoveMember(string userId)
        {
            int initialCount = Members.Count;
            Members.RemoveAll(m => m.UserId == userId);
            
            return Members.Count < initialCount;
        }
    }

    [Serializable]
    public class TeamMember
    {
        public string UserId { get; set; }
        public TeamRole Role { get; set; }
    }
    
    public enum TeamRole 
    { 
        Member, 
        Admin 
    }
} 