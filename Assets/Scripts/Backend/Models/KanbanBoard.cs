using System;
using System.Collections.Generic;
using System.Linq;

namespace Orion.Models
{
    [Serializable]
    public class KanbanBoard
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<KanbanColumn> Columns { get; set; } = new List<KanbanColumn>();
        public List<string> MemberIds { get; set; } = new List<string>();
        
        // Методы для управления доской
        public void AddColumn(string title)
        {
            Columns.Add(new KanbanColumn
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Cards = new List<KanbanCard>()
            });
        }
        
        public bool RemoveColumn(string columnId)
        {
            return Columns.RemoveAll(c => c.Id == columnId) > 0;
        }
        
        public bool MoveCard(string cardId, string sourceColumnId, string targetColumnId, int targetIndex)
        {
            var sourceColumn = Columns.FirstOrDefault(c => c.Id == sourceColumnId);
            var targetColumn = Columns.FirstOrDefault(c => c.Id == targetColumnId);
            
            if (sourceColumn == null || targetColumn == null)
                return false;
                
            var card = sourceColumn.Cards.FirstOrDefault(card => card.Id == cardId);
            
            if (card == null)
                return false;
                
            sourceColumn.Cards.Remove(card);
            
            if (targetIndex >= 0 && targetIndex <= targetColumn.Cards.Count)
                targetColumn.Cards.Insert(targetIndex, card);
            else
                targetColumn.Cards.Add(card);
                
            return true;
        }
    }

    [Serializable]
    public class KanbanColumn
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<KanbanCard> Cards { get; set; } = new List<KanbanCard>();
        
        public KanbanCard AddCard(string title, string description = "")
        {
            var card = new KanbanCard
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Description = description,
                CreatedAt = DateTime.Now
            };
            
            Cards.Add(card);
            return card;
        }
    }

    [Serializable]
    public class KanbanCard
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> AssigneeIds { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        
        public void AssignUser(string userId)
        {
            if (!AssigneeIds.Contains(userId))
                AssigneeIds.Add(userId);
        }
        
        public void UnassignUser(string userId)
        {
            AssigneeIds.Remove(userId);
        }
        
        public void AddTag(string tag)
        {
            if (!Tags.Contains(tag))
                Tags.Add(tag);
        }
        
        public void RemoveTag(string tag)
        {
            Tags.Remove(tag);
        }
    }
} 