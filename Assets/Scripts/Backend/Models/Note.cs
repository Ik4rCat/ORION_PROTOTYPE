using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Orion.Models
{
    [Serializable]
    public class Note
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        
        // Связи с другими заметками
        public List<string> LinkedNoteIds { get; set; } = new List<string>();
        
        // Регулярное выражение для поиска ссылок в формате [[Название заметки]]
        private static readonly Regex LinkRegex = new Regex(@"\[\[(.*?)\]\]", RegexOptions.Compiled);
        
        // Парсинг контента для извлечения ссылок на другие заметки
        public List<string> ExtractLinkTitles()
        {
            if (string.IsNullOrEmpty(Content))
                return new List<string>();
                
            var matches = LinkRegex.Matches(Content);
            var linkTitles = new List<string>();
            
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    linkTitles.Add(match.Groups[1].Value);
                }
            }
            
            return linkTitles;
        }
        
        // Добавление тега
        public void AddTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag) && !Tags.Contains(tag))
                Tags.Add(tag);
        }
        
        // Удаление тега
        public bool RemoveTag(string tag)
        {
            return Tags.Remove(tag);
        }
        
        // Обновление содержимого заметки
        public void UpdateContent(string newContent)
        {
            Content = newContent;
            ModifiedAt = DateTime.Now;
        }
    }

    [Serializable]
    public class NoteCollection
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> NoteIds { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        
        // Добавление заметки в коллекцию
        public void AddNote(string noteId)
        {
            if (!NoteIds.Contains(noteId))
            {
                NoteIds.Add(noteId);
                ModifiedAt = DateTime.Now;
            }
        }
        
        // Удаление заметки из коллекции
        public bool RemoveNote(string noteId)
        {
            bool result = NoteIds.Remove(noteId);
            if (result)
                ModifiedAt = DateTime.Now;
                
            return result;
        }
    }
} 