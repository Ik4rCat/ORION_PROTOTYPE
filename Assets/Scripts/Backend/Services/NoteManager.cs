using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Models;
using Orion.Backend.Services;
using Orion.Backend.Models;

namespace Orion.Services
{
    public class NoteManager : MonoBehaviour
    {
        public static NoteManager Instance { get; private set; }
        
        // Словарь заметок (в реальном приложении будет загружаться из базы данных)
        private Dictionary<string, Note> _notesById = new Dictionary<string, Note>();
        
        // Словарь коллекций заметок
        private Dictionary<string, NoteCollection> _collectionsById = new Dictionary<string, NoteCollection>();
        
        // События
        public event Action<Note> OnNoteCreated;
        public event Action<Note> OnNoteUpdated;
        public event Action<string> OnNoteDeleted;
        public event Action<NoteCollection> OnCollectionCreated;
        public event Action<NoteCollection> OnCollectionUpdated;
        public event Action<string> OnCollectionDeleted;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        // Создание новой заметки
        public Note CreateNote(string title, string content = "", List<string> tags = null, string teamId = null)
        {
            var note = new Note
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Content = content,
                Tags = tags ?? new List<string>(),
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };
            
            _notesById[note.Id] = note;
            
            // Если указан ID команды, добавляем заметку к команде
            if (!string.IsNullOrEmpty(teamId))
            {
                var team = TeamManager.Instance.GetTeamById(teamId);
                if (team != null)
                {
                    team.NoteIds.Add(note.Id);
                }
            }
            
            OnNoteCreated?.Invoke(note);
            
            return note;
        }
        
        // Обновление содержимого заметки
        public bool UpdateNoteContent(string noteId, string newContent)
        {
            if (!_notesById.TryGetValue(noteId, out Note note))
                return false;
                
            note.UpdateContent(newContent);
            
            // Обновляем связи с другими заметками
            UpdateNoteLinks(note);
            
            OnNoteUpdated?.Invoke(note);
            
            return true;
        }
        
        // Обновление заголовка заметки
        public bool UpdateNoteTitle(string noteId, string newTitle)
        {
            if (!_notesById.TryGetValue(noteId, out Note note))
                return false;
                
            note.Title = newTitle;
            note.ModifiedAt = DateTime.Now;
            
            OnNoteUpdated?.Invoke(note);
            
            return true;
        }
        
        // Добавление тега к заметке
        public bool AddTagToNote(string noteId, string tag)
        {
            if (!_notesById.TryGetValue(noteId, out Note note))
                return false;
                
            note.AddTag(tag);
            OnNoteUpdated?.Invoke(note);
            
            return true;
        }
        
        // Удаление тега из заметки
        public bool RemoveTagFromNote(string noteId, string tag)
        {
            if (!_notesById.TryGetValue(noteId, out Note note))
                return false;
                
            var result = note.RemoveTag(tag);
            
            if (result)
                OnNoteUpdated?.Invoke(note);
                
            return result;
        }
        
        // Удаление заметки
        public bool DeleteNote(string noteId)
        {
            if (!_notesById.TryGetValue(noteId, out Note note))
                return false;
                
            _notesById.Remove(noteId);
            
            // Удаляем ID заметки из команд
            if (UserManager.Instance.CurrentUser != null)
            {
                var userTeams = TeamManager.Instance.GetUserTeams(UserManager.Instance.CurrentUser.Id);
                foreach (var team in userTeams)
                {
                    if (team.NoteIds.Contains(noteId))
                    {
                        team.NoteIds.Remove(noteId);
                    }
                }
            }
            
            // Удаляем заметку из всех коллекций
            foreach (var collection in _collectionsById.Values)
            {
                if (collection.NoteIds.Contains(noteId))
                {
                    collection.RemoveNote(noteId);
                    OnCollectionUpdated?.Invoke(collection);
                }
            }
            
            OnNoteDeleted?.Invoke(noteId);
            
            return true;
        }
        
        // Получение заметки по ID
        public Note GetNoteById(string noteId)
        {
            return _notesById.TryGetValue(noteId, out Note note) ? note : null;
        }
        
        // Создание новой коллекции заметок
        public NoteCollection CreateCollection(string title, List<string> noteIds = null, string teamId = null)
        {
            var collection = new NoteCollection
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                NoteIds = noteIds ?? new List<string>(),
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };
            
            _collectionsById[collection.Id] = collection;
            
            // Если указан ID команды, добавляем коллекцию к команде
            if (!string.IsNullOrEmpty(teamId))
            {
                var team = TeamManager.Instance.GetTeamById(teamId);
                if (team != null)
                {
                    // В данном примере мы не храним IDs коллекций в команде, 
                    // но можно расширить класс Team для этой функциональности
                }
            }
            
            OnCollectionCreated?.Invoke(collection);
            
            return collection;
        }
        
        // Добавление заметки в коллекцию
        public bool AddNoteToCollection(string collectionId, string noteId)
        {
            if (!_collectionsById.TryGetValue(collectionId, out NoteCollection collection))
                return false;
                
            if (!_notesById.ContainsKey(noteId))
                return false;
                
            collection.AddNote(noteId);
            OnCollectionUpdated?.Invoke(collection);
            
            return true;
        }
        
        // Удаление заметки из коллекции
        public bool RemoveNoteFromCollection(string collectionId, string noteId)
        {
            if (!_collectionsById.TryGetValue(collectionId, out NoteCollection collection))
                return false;
                
            var result = collection.RemoveNote(noteId);
            
            if (result)
                OnCollectionUpdated?.Invoke(collection);
                
            return result;
        }
        
        // Удаление коллекции
        public bool DeleteCollection(string collectionId)
        {
            if (!_collectionsById.TryGetValue(collectionId, out NoteCollection collection))
                return false;
                
            _collectionsById.Remove(collectionId);
            
            OnCollectionDeleted?.Invoke(collectionId);
            
            return true;
        }
        
        // Получение коллекции по ID
        public NoteCollection GetCollectionById(string collectionId)
        {
            return _collectionsById.TryGetValue(collectionId, out NoteCollection collection) ? collection : null;
        }
        
        // Получение списка заметок команды
        public List<Note> GetTeamNotes(string teamId)
        {
            var team = TeamManager.Instance.GetTeamById(teamId);
            
            if (team == null)
                return new List<Note>();
                
            return team.NoteIds.Where(id => _notesById.ContainsKey(id))
                              .Select(id => _notesById[id])
                              .ToList();
        }
        
        // Получение списка всех заметок, доступных пользователю
        public List<Note> GetUserNotes(string userId)
        {
            var userTeams = TeamManager.Instance.GetUserTeams(userId);
            var noteIds = new HashSet<string>();
            
            foreach (var team in userTeams)
            {
                foreach (var noteId in team.NoteIds)
                {
                    noteIds.Add(noteId);
                }
            }
            
            return noteIds.Where(id => _notesById.ContainsKey(id))
                          .Select(id => _notesById[id])
                          .ToList();
        }
        
        // Поиск заметок по тегам
        public List<Note> FindNotesByTags(List<string> tags)
        {
            if (tags == null || tags.Count == 0)
                return new List<Note>();
                
            return _notesById.Values.Where(note => 
                tags.All(tag => note.Tags.Contains(tag))).ToList();
        }
        
        // Поиск заметок по тексту
        public List<Note> SearchNotes(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return new List<Note>();
                
            searchText = searchText.ToLower();
            
            return _notesById.Values.Where(note => 
                note.Title.ToLower().Contains(searchText) || 
                note.Content.ToLower().Contains(searchText)).ToList();
        }
        
        // Обновление связей заметки с другими заметками
        private void UpdateNoteLinks(Note note)
        {
            // Получаем названия заметок из контента
            var linkTitles = note.ExtractLinkTitles();
            
            // Находим соответствующие заметки по названию
            var linkedNoteIds = new List<string>();
            
            foreach (var title in linkTitles)
            {
                var linkedNote = _notesById.Values.FirstOrDefault(n => 
                    n.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                    
                if (linkedNote != null)
                {
                    linkedNoteIds.Add(linkedNote.Id);
                }
            }
            
            note.LinkedNoteIds = linkedNoteIds;
        }
        
        // Получение графа связей между заметками
        public Dictionary<string, List<string>> GetNoteGraph()
        {
            var graph = new Dictionary<string, List<string>>();
            
            foreach (var note in _notesById.Values)
            {
                graph[note.Id] = note.LinkedNoteIds.ToList();
            }
            
            return graph;
        }
    }
} 