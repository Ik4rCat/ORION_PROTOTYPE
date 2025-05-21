using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Orion.Models
{
    [Serializable]
    public class Canvas
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<CanvasElement> Elements { get; set; } = new List<CanvasElement>();
        public Vector2 Size { get; set; } = new Vector2(5000, 5000);
        
        // Методы для управления элементами на холсте
        public void AddElement(CanvasElement element)
        {
            if (element == null)
                return;
                
            if (string.IsNullOrEmpty(element.Id))
                element.Id = Guid.NewGuid().ToString();
                
            Elements.Add(element);
        }
        
        public bool RemoveElement(string elementId)
        {
            return Elements.RemoveAll(e => e.Id == elementId) > 0;
        }
        
        public CanvasElement GetElement(string elementId)
        {
            return Elements.FirstOrDefault(e => e.Id == elementId);
        }
        
        public List<ConnectionElement> GetConnections()
        {
            return Elements.OfType<ConnectionElement>().ToList();
        }
    }

    [Serializable]
    public abstract class CanvasElement
    {
        public string Id { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public float Rotation { get; set; }
        public Color Color { get; set; } = Color.white;
        
        public abstract void Render();
    }

    [Serializable]
    public class TextElement : CanvasElement
    {
        public string Text { get; set; }
        public float FontSize { get; set; } = 14f;
        public TextAnchor Alignment { get; set; } = TextAnchor.MiddleCenter;
        
        public override void Render()
        {
            // Реализация рендеринга текстового элемента
        }
    }

    [Serializable]
    public class ImageElement : CanvasElement
    {
        public string ImagePath { get; set; }
        public Color TintColor { get; set; } = Color.white;
        
        public override void Render()
        {
            // Реализация рендеринга изображения
        }
    }

    [Serializable]
    public class NoteElement : CanvasElement
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public Color BackgroundColor { get; set; } = new Color(1f, 0.92f, 0.23f, 1f); // Желтый стикер
        
        public override void Render()
        {
            // Реализация рендеринга заметки
        }
    }

    [Serializable]
    public class ShapeElement : CanvasElement
    {
        public ShapeType Type { get; set; } = ShapeType.Rectangle;
        public bool IsFilled { get; set; } = true;
        public float StrokeWidth { get; set; } = 2f;
        public Color StrokeColor { get; set; } = Color.black;
        
        public enum ShapeType 
        { 
            Rectangle, 
            Circle, 
            Triangle, 
            Hexagon,
            Star
        }
        
        public override void Render()
        {
            // Реализация рендеринга фигуры
        }
    }

    [Serializable]
    public class ConnectionElement : CanvasElement
    {
        public string SourceElementId { get; set; }
        public string TargetElementId { get; set; }
        public LineType LineStyle { get; set; } = LineType.Straight;
        public float LineWidth { get; set; } = 2f;
        public bool HasArrow { get; set; } = false;
        
        public enum LineType 
        { 
            Straight, 
            Curved, 
            Orthogonal
        }
        
        public override void Render()
        {
            // Реализация рендеринга линии соединения
        }
    }
} 