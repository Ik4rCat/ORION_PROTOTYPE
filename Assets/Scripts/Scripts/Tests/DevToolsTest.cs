using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Orion.Core;
using System.Text;
using System.Collections.Generic;

namespace Orion.Tests
{
    public class DevToolsTest : MonoBehaviour
    {
        [Header("Вкладки")]
        [SerializeField] private GameObject[] tabPanels;
        [SerializeField] private Toggle[] tabToggles;
        
        [Header("Анализ проекта")]
        [SerializeField] private TMP_InputField projectPathInput;
        [SerializeField] private Button analyzeButton;
        [SerializeField] private TextMeshProUGUI analysisResultText;
        
        [Header("Документация")]
        [SerializeField] private TMP_InputField sourcePathInput;
        [SerializeField] private TMP_InputField outputPathInput;
        [SerializeField] private Button generateButton;
        [SerializeField] private TextMeshProUGUI docStatusText;
        
        [Header("Визуализация зависимостей")]
        [SerializeField] private RawImage canvasContainer;
        [SerializeField] private Button visualizeButton;
        
        private DeveloperTools _devTools;
        
        private void Start()
        {
            _devTools = FindFirstObjectByType<DeveloperTools>();
            if (_devTools == null)
            {
                Debug.LogError("DeveloperTools не найдены на сцене!");
                return;
            }
            
            // Устанавливаем пути по умолчанию
            projectPathInput.text = Application.dataPath;
            sourcePathInput.text = Application.dataPath;
            outputPathInput.text = Application.persistentDataPath + "/Docs";
            
            // Настраиваем вкладки
            for (int i = 0; i < tabToggles.Length && i < tabPanels.Length; i++)
            {
                int index = i; // Для использования в лямбда-выражении
                tabToggles[i].onValueChanged.AddListener((isOn) => {
                    if (isOn) ShowTab(index);
                });
            }
            
            // По умолчанию показываем первую вкладку
            if (tabToggles.Length > 0)
                tabToggles[0].isOn = true;
            
            // Настраиваем кнопки
            analyzeButton.onClick.AddListener(TestAnalyzeProject);
            generateButton.onClick.AddListener(TestGenerateDocumentation);
            visualizeButton.onClick.AddListener(TestVisualizeDependencies);
            
            Debug.Log("DevToolsTest инициализирован");
        }
        
        private void ShowTab(int index)
        {
            for (int i = 0; i < tabPanels.Length; i++)
            {
                tabPanels[i].SetActive(i == index);
            }
        }
        
        public async void TestAnalyzeProject()
        {
            string path = projectPathInput.text;
            
            if (string.IsNullOrEmpty(path))
            {
                analysisResultText.text = "Укажите путь к проекту";
                return;
            }
            
            analysisResultText.text = "Анализ проекта...";
            analyzeButton.interactable = false;
            
            var result = await _devTools.AnalyzeProjectStructure(path);
            
            analyzeButton.interactable = true;
            
            if (result == null)
            {
                analysisResultText.text = "Ошибка анализа проекта. Проверьте консоль Unity.";
                Debug.LogError("Ошибка анализа проекта. Возможно, у вас нет прав доступа, или DeveloperTools не настроены правильно.");
                return;
            }
            
            // Формируем отчет
            var sb = new StringBuilder();
            sb.AppendLine("АНАЛИЗ ПРОЕКТА");
            sb.AppendLine("==============");
            sb.AppendLine($"Файлы в проекте:");
            sb.AppendLine($"- Скрипты: {result.ScriptCount}");
            sb.AppendLine($"- Сцены: {result.SceneCount}");
            sb.AppendLine($"- Префабы: {result.PrefabCount}");
            sb.AppendLine();
            
            sb.AppendLine("Пространства имен:");
            foreach (var ns in result.ScriptsByNamespace)
            {
                sb.AppendLine($"- {ns.Key}: {ns.Value} скриптов");
            }
            
            analysisResultText.text = sb.ToString();
            Debug.Log("Анализ проекта успешно завершен");
        }
        
        public async void TestGenerateDocumentation()
        {
            string sourcePath = sourcePathInput.text;
            string outputPath = outputPathInput.text;
            
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(outputPath))
            {
                docStatusText.text = "Укажите пути для исходного кода и выходной документации";
                return;
            }
            
            docStatusText.text = "Генерация документации...";
            generateButton.interactable = false;
            
            await _devTools.GenerateCodeDocumentation(sourcePath, outputPath);
            
            generateButton.interactable = true;
            docStatusText.text = $"Документация сгенерирована в папке: {outputPath}";
            Debug.Log($"Документация сгенерирована в папке: {outputPath}");
        }
        
        public void TestVisualizeDependencies()
        {
            // Этот метод требует дополнительной реализации для отображения 
            // визуализации на UI элементе. Сейчас просто вызываем метод.
            
            // Создаем временный холст
            var canvas = new Orion.Models.Canvas
            {
                Id = System.Guid.NewGuid().ToString(),
                Title = "Визуализация зависимостей",
                Size = new Vector2(800, 600)
            };
            
            _devTools.VisualizeComponentDependencies(canvas);
            
            Debug.Log("Визуализация зависимостей вызвана. Для полной реализации требуется дополнительный код для отображения результатов.");
        }
    }
} 