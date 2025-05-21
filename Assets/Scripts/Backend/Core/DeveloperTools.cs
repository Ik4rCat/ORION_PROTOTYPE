using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Models;
using Orion.Services;
using ModelsCanvas = Orion.Models.Canvas; // Добавляем псевдоним
using Orion.Backend.Services;
using Orion.Backend.Models;

namespace Orion.Core
{
    /// <summary>
    /// Класс, содержащий инструменты для разработчиков игр.
    /// Доступен только для пользователей с ролью Developer.
    /// </summary>
    public class DeveloperTools : MonoBehaviour
    {
        // Событие, вызываемое при завершении анализа структуры проекта
        public event Action<ProjectStructureAnalysis> OnProjectAnalysisCompleted;
        
        // Событие, вызываемое при создании визуализации зависимостей компонентов
        public event Action<string> OnDependencyVisualizationCompleted;
        
        /// <summary>
        /// Проверяет, имеет ли текущий пользователь доступ к инструментам разработчика
        /// </summary>
        public bool IsAvailable => UserManager.Instance.CurrentUser?.HasDeveloperAccess ?? false;
        
        // Хранение последнего анализа проекта
        private ProjectStructureAnalysis _lastAnalysis;
        
        /// <summary>
        /// Анализирует структуру проекта Unity
        /// </summary>
        /// <param name="projectPath">Путь к проекту Unity</param>
        /// <returns>Результат анализа структуры проекта</returns>
        public async Task<ProjectStructureAnalysis> AnalyzeProjectStructure(string projectPath)
        {
            if (!IsAvailable)
            {
                Debug.LogWarning("Инструменты разработчика недоступны для текущего пользователя");
                return null;
            }
            
            if (!Directory.Exists(projectPath))
            {
                Debug.LogError($"Каталог проекта не найден: {projectPath}");
                return null;
            }
            
            // Создаем новый анализ
            var analysis = new ProjectStructureAnalysis();
            
            // Запускаем анализ в отдельном потоке
            analysis = await Task.Run(() => 
            {
                try
                {
                    // Анализ скриптов
                    var scriptFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
                    var currentAnalysis = new ProjectStructureAnalysis();
                    currentAnalysis.ScriptCount = scriptFiles.Length;
                    
                    // Анализ сцен
                    var sceneFiles = Directory.GetFiles(projectPath, "*.unity", SearchOption.AllDirectories);
                    currentAnalysis.SceneCount = sceneFiles.Length;
                    
                    // Анализ префабов
                    var prefabFiles = Directory.GetFiles(projectPath, "*.prefab", SearchOption.AllDirectories);
                    currentAnalysis.PrefabCount = prefabFiles.Length;
                    
                    // Анализ namespace
                    currentAnalysis.ScriptsByNamespace = AnalyzeNamespaces(scriptFiles);
                    
                    // Анализ неиспользуемых ассетов и отсутствующих ссылок
                    // (В реальном приложении здесь будет более сложный анализ)
                    currentAnalysis.UnusedAssets = new List<string>();
                    currentAnalysis.MissingReferences = new List<string>();

                    return currentAnalysis; // Возвращаем результат
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Ошибка при анализе проекта: {ex.Message}");
                    return null; // Возвращаем null в случае ошибки
                }
            });
            
            _lastAnalysis = analysis;
            OnProjectAnalysisCompleted?.Invoke(analysis);
            
            return analysis;
        }
        
        /// <summary>
        /// Анализирует namespace в файлах скриптов
        /// </summary>
        private Dictionary<string, int> AnalyzeNamespaces(string[] scriptFiles)
        {
            var namespaces = new Dictionary<string, int>();
            
            foreach (var file in scriptFiles)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var nsMatch = System.Text.RegularExpressions.Regex.Match(content, @"namespace\s+([^\s{]+)");
                    
                    if (nsMatch.Success)
                    {
                        var ns = nsMatch.Groups[1].Value;
                        
                        if (namespaces.ContainsKey(ns))
                            namespaces[ns]++;
                        else
                            namespaces[ns] = 1;
                    }
                    else
                    {
                        // Скрипты без namespace
                        if (namespaces.ContainsKey("(default)"))
                            namespaces["(default)"]++;
                        else
                            namespaces["(default)"] = 1;
                    }
                }
                catch
                {
                    // Игнорируем ошибки при чтении файла
                }
            }
            
            return namespaces;
        }
        
        /// <summary>
        /// Генерирует документацию по коду проекта
        /// </summary>
        /// <param name="sourcePath">Путь к исходному коду</param>
        /// <param name="outputPath">Путь для сохранения документации</param>
        public async Task GenerateCodeDocumentation(string sourcePath, string outputPath)
        {
            if (!IsAvailable)
            {
                Debug.LogWarning("Инструменты разработчика недоступны для текущего пользователя");
                return;
            }
            
            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"Каталог с исходным кодом не найден: {sourcePath}");
                return;
            }
            
            // Создаем выходной каталог, если он не существует
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
                
            // Генерация документации (в реальном приложении здесь будет использоваться
            // специализированная библиотека для генерации документации)
            await Task.Run(() => 
            {
                try
                {
                    // Получаем все файлы .cs
                    var scriptFiles = Directory.GetFiles(sourcePath, "*.cs", SearchOption.AllDirectories);
                    
                    // Создаем индексный файл
                    string indexContent = "# Документация по проекту\n\n";
                    indexContent += "## Обзор\n\n";
                    indexContent += $"Всего файлов: {scriptFiles.Length}\n\n";
                    indexContent += "## Файлы\n\n";
                    
                    foreach (var scriptFile in scriptFiles)
                    {
                        // Получаем название файла без пути
                        string fileName = Path.GetFileName(scriptFile);
                        string relPath = scriptFile.Substring(sourcePath.Length).TrimStart('\\', '/');
                        
                        // Добавляем в индекс
                        indexContent += $"- [{fileName}]({relPath.Replace('\\', '/')}.md)\n";
                        
                        // Генерируем документацию для каждого файла
                        string fileContent = File.ReadAllText(scriptFile);
                        string docContent = GenerateFileDocumentation(fileName, fileContent);
                        
                        // Определяем путь для файла документации
                        string docFilePath = Path.Combine(outputPath, relPath + ".md");
                        
                        // Создаем подкаталоги, если необходимо
                        Directory.CreateDirectory(Path.GetDirectoryName(docFilePath));
                        
                        // Сохраняем документацию
                        File.WriteAllText(docFilePath, docContent);
                    }
                    
                    // Сохраняем индексный файл
                    File.WriteAllText(Path.Combine(outputPath, "index.md"), indexContent);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Ошибка при генерации документации: {ex.Message}");
                }
            });
        }
        
        /// <summary>
        /// Генерирует документацию для отдельного файла
        /// </summary>
        private string GenerateFileDocumentation(string fileName, string content)
        {
            string doc = $"# {fileName}\n\n";
            
            // Поиск класса и описания
            var classMatch = System.Text.RegularExpressions.Regex.Match(content, 
                @"///\s*<summary>(.*?)</summary>.*?(?:public|internal|private)\s+(?:class|struct|enum|interface)\s+(\w+)",
                System.Text.RegularExpressions.RegexOptions.Singleline);
                
            if (classMatch.Success)
            {
                string className = classMatch.Groups[2].Value;
                string classDesc = classMatch.Groups[1].Value.Trim();
                
                doc += $"## Класс {className}\n\n";
                doc += $"{classDesc}\n\n";
            }
            
            // Поиск методов и их описаний
            var methodMatches = System.Text.RegularExpressions.Regex.Matches(content,
                @"///\s*<summary>(.*?)</summary>.*?(?:public|internal|private|protected)\s+(?:static\s+)?\w+\s+(\w+)\s*\((.*?)\)",
                System.Text.RegularExpressions.RegexOptions.Singleline);
                
            if (methodMatches.Count > 0)
            {
                doc += "## Методы\n\n";
                
                foreach (System.Text.RegularExpressions.Match match in methodMatches)
                {
                    string methodName = match.Groups[2].Value;
                    string methodDesc = match.Groups[1].Value.Trim();
                    string parameters = match.Groups[3].Value.Trim();
                    
                    doc += $"### {methodName}({parameters})\n\n";
                    doc += $"{methodDesc}\n\n";
                }
            }
            
            return doc;
        }
        
        /// <summary>
        /// Визуализирует зависимости между компонентами проекта на холсте
        /// </summary>
        /// <param name="targetCanvas">Холст для визуализации</param>
        public void VisualizeComponentDependencies(ModelsCanvas targetCanvas)
        {
            if (!IsAvailable)
            {
                Debug.LogWarning("Инструменты разработчика недоступны для текущего пользователя");
                return;
            }
            
            if (_lastAnalysis == null)
            {
                Debug.LogWarning("Необходимо сначала провести анализ проекта");
                return;
            }
            
            if (targetCanvas == null)
            {
                Debug.LogError("Целевой холст не указан");
                return;
            }
            
            // Очищаем холст
            foreach (var element in targetCanvas.Elements.ToList())
            {
                targetCanvas.RemoveElement(element.Id);
            }
            
            // Создаем визуализацию namespace
            int namespaceCount = _lastAnalysis.ScriptsByNamespace.Count;
            float angle = 0;
            float radius = 200;
            Vector2 center = new Vector2(targetCanvas.Size.x / 2, targetCanvas.Size.y / 2);
            
            // Создаем элементы для каждого namespace
            Dictionary<string, string> namespaceElementIds = new Dictionary<string, string>();
            
            foreach (var ns in _lastAnalysis.ScriptsByNamespace)
            {
                // Вычисляем позицию на окружности
                float x = center.x + Mathf.Cos(angle) * radius;
                float y = center.y + Mathf.Sin(angle) * radius;
                Vector2 position = new Vector2(x, y);
                
                // Создаем элемент для namespace
                var element = new ShapeElement
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = ShapeElement.ShapeType.Rectangle,
                    Position = position,
                    Size = new Vector2(100, 50),
                    Color = new Color(0.2f, 0.6f, 0.9f, 0.8f)
                };
                
                targetCanvas.AddElement(element);
                
                // Добавляем текстовый элемент с названием namespace
                var textElement = new TextElement
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = $"{ns.Key}\n({ns.Value})",
                    Position = position,
                    Size = new Vector2(100, 50),
                    FontSize = 12
                };
                
                targetCanvas.AddElement(textElement);
                
                // Сохраняем ID элемента для последующего соединения
                namespaceElementIds[ns.Key] = element.Id;
                
                // Увеличиваем угол для следующего элемента
                angle += 2 * Mathf.PI / namespaceCount;
            }
            
            // Добавляем центральный элемент
            var centerElement = new ShapeElement
            {
                Id = Guid.NewGuid().ToString(),
                Type = ShapeElement.ShapeType.Circle,
                Position = center,
                Size = new Vector2(80, 80),
                Color = new Color(0.9f, 0.3f, 0.3f, 0.8f)
            };
            
            targetCanvas.AddElement(centerElement);
            
            var centerText = new TextElement
            {
                Id = Guid.NewGuid().ToString(),
                Text = "Проект",
                Position = center,
                Size = new Vector2(80, 80),
                FontSize = 14
            };
            
            targetCanvas.AddElement(centerText);
            
            // Соединяем центральный элемент с namespace
            foreach (var nsId in namespaceElementIds.Values)
            {
                var connection = new ConnectionElement
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceElementId = centerElement.Id,
                    TargetElementId = nsId,
                    LineStyle = ConnectionElement.LineType.Straight,
                    LineWidth = 2f,
                    HasArrow = true
                };
                
                targetCanvas.AddElement(connection);
            }
            
            // Вызываем событие завершения
            OnDependencyVisualizationCompleted?.Invoke(targetCanvas.Id);
        }
    }
    
    /// <summary>
    /// Класс для хранения результатов анализа структуры проекта
    /// </summary>
    [Serializable]
    public class ProjectStructureAnalysis
    {
        public int ScriptCount { get; set; }
        public int SceneCount { get; set; }
        public int PrefabCount { get; set; }
        public Dictionary<string, int> ScriptsByNamespace { get; set; } = new Dictionary<string, int>();
        public List<string> UnusedAssets { get; set; } = new List<string>();
        public List<string> MissingReferences { get; set; } = new List<string>();
    }
} 