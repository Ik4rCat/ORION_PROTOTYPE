using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Models;
using Orion.Backend.Services;
using Orion.Backend.Models;
using ModelsCanvas = Orion.Models.Canvas;

namespace Orion.Services
{
    public class CanvasManager : MonoBehaviour
    {
        public static CanvasManager Instance { get; private set; }
        
        // Список холстов (в реальном приложении будет загружаться из базы данных)
        private List<ModelsCanvas> _canvases = new List<ModelsCanvas>();
        
        // События
        public event Action<ModelsCanvas> OnCanvasCreated;
        public event Action<ModelsCanvas> OnCanvasUpdated;
        public event Action<string> OnCanvasDeleted;
        
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
        
        // Создание нового холста
        public ModelsCanvas CreateCanvas(string title, string teamId = null)
        {
            var canvas = new ModelsCanvas
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Elements = new List<CanvasElement>()
            };
            
            _canvases.Add(canvas);
            
            // Если указан ID команды, добавляем холст к команде
            if (!string.IsNullOrEmpty(teamId))
            {
                var team = TeamManager.Instance.GetTeamById(teamId);
                if (team != null)
                {
                    team.CanvasIds.Add(canvas.Id);
                }
            }
            
            OnCanvasCreated?.Invoke(canvas);
            
            return canvas;
        }
        
        // Удаление холста
        public bool DeleteCanvas(string canvasId)
        {
            var canvas = GetCanvasById(canvasId);
            
            if (canvas == null)
                return false;
                
            _canvases.Remove(canvas);
            
            // Если холст привязан к команде, удаляем ID холста из списка команды
            if (UserManager.Instance.CurrentUser != null)
            {
                var userTeams = TeamManager.Instance.GetUserTeams(UserManager.Instance.CurrentUser.Id);
                foreach (var team in userTeams)
                {
                    if (team.CanvasIds.Contains(canvasId))
                    {
                        team.CanvasIds.Remove(canvasId);
                    }
                }
            }
            
            OnCanvasDeleted?.Invoke(canvasId);
            
            return true;
        }
        
        // Добавление элемента на холст
        public bool AddElement(string canvasId, CanvasElement element)
        {
            var canvas = GetCanvasById(canvasId);
            
            if (canvas == null)
                return false;
                
            canvas.AddElement(element);
            OnCanvasUpdated?.Invoke(canvas);
            
            return true;
        }
        
        // Добавление текстового элемента
        public TextElement AddTextElement(string canvasId, string text, Vector2 position, float fontSize = 14f)
        {
            var element = new TextElement
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Position = position,
                Size = new Vector2(200, 100),
                FontSize = fontSize
            };
            
            if (AddElement(canvasId, element))
                return element;
                
            return null;
        }
        
        // Добавление элемента изображения
        public ImageElement AddImageElement(string canvasId, string imagePath, Vector2 position, Vector2 size)
        {
            var element = new ImageElement
            {
                Id = Guid.NewGuid().ToString(),
                ImagePath = imagePath,
                Position = position,
                Size = size
            };
            
            if (AddElement(canvasId, element))
                return element;
                
            return null;
        }
        
        // Добавление элемента заметки
        public NoteElement AddNoteElement(string canvasId, string title, string content, Vector2 position)
        {
            var element = new NoteElement
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Content = content,
                Position = position,
                Size = new Vector2(200, 200)
            };
            
            if (AddElement(canvasId, element))
                return element;
                
            return null;
        }
        
        // Добавление элемента фигуры
        public ShapeElement AddShapeElement(string canvasId, ShapeElement.ShapeType type, Vector2 position, Vector2 size, Color color)
        {
            var element = new ShapeElement
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Position = position,
                Size = size,
                Color = color
            };
            
            if (AddElement(canvasId, element))
                return element;
                
            return null;
        }
        
        // Добавление элемента соединения
        public ConnectionElement AddConnectionElement(string canvasId, string sourceElementId, string targetElementId, ConnectionElement.LineType lineStyleValue = ConnectionElement.LineType.Straight)
        {
            var element = new ConnectionElement
            {
                Id = Guid.NewGuid().ToString(),
                SourceElementId = sourceElementId,
                TargetElementId = targetElementId,
                LineStyle = lineStyleValue
            };
            
            if (AddElement(canvasId, element))
                return element;
                
            return null;
        }
        
        // Удаление элемента с холста
        public bool RemoveElement(string canvasId, string elementId)
        {
            var canvas = GetCanvasById(canvasId);
            
            if (canvas == null)
                return false;
                
            var result = canvas.RemoveElement(elementId);
            
            // Если это был элемент соединения, удаляем все связи
            var connections = canvas.GetConnections().Where(c => 
                c.SourceElementId == elementId || c.TargetElementId == elementId).ToList();
                
            foreach (var connection in connections)
            {
                canvas.RemoveElement(connection.Id);
            }
            
            if (result)
                OnCanvasUpdated?.Invoke(canvas);
                
            return result;
        }
        
        // Обновление позиции элемента
        public bool UpdateElementPosition(string canvasId, string elementId, Vector2 newPosition)
        {
            var canvas = GetCanvasById(canvasId);
            
            if (canvas == null)
                return false;
                
            var element = canvas.GetElement(elementId);
            
            if (element == null)
                return false;
                
            element.Position = newPosition;
            OnCanvasUpdated?.Invoke(canvas);
            
            return true;
        }
        
        // Обновление размера элемента
        public bool UpdateElementSize(string canvasId, string elementId, Vector2 newSize)
        {
            var canvas = GetCanvasById(canvasId);
            
            if (canvas == null)
                return false;
                
            var element = canvas.GetElement(elementId);
            
            if (element == null)
                return false;
                
            element.Size = newSize;
            OnCanvasUpdated?.Invoke(canvas);
            
            return true;
        }
        
        // Получение холста по ID
        public ModelsCanvas GetCanvasById(string canvasId)
        {
            return _canvases.FirstOrDefault(c => c.Id == canvasId);
        }
        
        // Получение списка холстов команды
        public List<ModelsCanvas> GetTeamCanvases(string teamId)
        {
            var team = TeamManager.Instance.GetTeamById(teamId);
            
            if (team == null)
                return new List<ModelsCanvas>();
                
            return _canvases.Where(c => team.CanvasIds.Contains(c.Id)).ToList();
        }
        
        // Получение списка всех холстов, доступных пользователю
        public List<ModelsCanvas> GetUserCanvases(string userId)
        {
            var userTeams = TeamManager.Instance.GetUserTeams(userId);
            var canvasIds = new HashSet<string>();
            
            foreach (var team in userTeams)
            {
                foreach (var canvasId in team.CanvasIds)
                {
                    canvasIds.Add(canvasId);
                }
            }
            
            return _canvases.Where(c => canvasIds.Contains(c.Id)).ToList();
        }
    }
} 