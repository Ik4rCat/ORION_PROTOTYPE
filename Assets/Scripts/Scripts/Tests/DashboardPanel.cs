using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Orion.Backend.Services;

namespace Orion.Tests
{
    public class DashboardPanel : MonoBehaviour
    {
        [Header("Элементы тестирования")]
        [SerializeField] private Button testSaveButton;
        [SerializeField] private TextMeshProUGUI testResultText;
        
        [Header("Информация о пользователе")]
        [SerializeField] private TextMeshProUGUI userInfoText;
        
        private void Start()
        {
            // Настройка кнопки тестирования сохранения
            if (testSaveButton != null)
            {
                testSaveButton.onClick.AddListener(TestSaveData);
            }
            
            // Обновляем информацию о пользователе
            UpdateUserInfo();
        }
        
        /// <summary>
        /// Обновляет информацию о текущем пользователе
        /// </summary>
        public void UpdateUserInfo()
        {
            if (userInfoText != null && UserManager.Instance != null)
            {
                var user = UserManager.Instance.CurrentUser;
                if (user != null)
                {
                    userInfoText.text = $"<b>Информация о пользователе:</b>\n" +
                                      $"ID: {user.Id}\n" +
                                      $"Имя пользователя: {user.Username}\n" +
                                      $"Email: {user.Email}\n" +
                                      $"Роль: {user.Role}\n" +
                                      $"Организация: {user.OrganizationId}";
                }
                else
                {
                    userInfoText.text = "Пользователь не авторизован";
                }
            }
        }
        
        /// <summary>
        /// Тестирует сохранение данных пользователя
        /// </summary>
        private void TestSaveData()
        {
            if (testResultText == null) return;
            
            // Проверяем наличие пользователей
            var usersPath = Path.Combine(Application.persistentDataPath, "users.json");
            var orgsPath = Path.Combine(Application.persistentDataPath, "organizations.json");
            var projectsPath = Path.Combine(Application.persistentDataPath, "projects.json");
            
            string result = "<b>Тест сохранения данных:</b>\n";
            
            // Проверка файла пользователей
            if (File.Exists(usersPath))
            {
                string usersJson = File.ReadAllText(usersPath);
                result += $"Файл пользователей: <color=green>НАЙДЕН</color>\n";
                result += $"Размер файла: {usersJson.Length} байт\n";
            }
            else
            {
                result += $"Файл пользователей: <color=red>НЕ НАЙДЕН</color>\n";
            }
            
            // Проверка файла организаций
            if (File.Exists(orgsPath))
            {
                string orgsJson = File.ReadAllText(orgsPath);
                result += $"Файл организаций: <color=green>НАЙДЕН</color>\n";
                result += $"Размер файла: {orgsJson.Length} байт\n";
            }
            else
            {
                result += $"Файл организаций: <color=red>НЕ НАЙДЕН</color>\n";
            }
            
            // Проверка файла проектов
            if (File.Exists(projectsPath))
            {
                string projectsJson = File.ReadAllText(projectsPath);
                result += $"Файл проектов: <color=green>НАЙДЕН</color>\n";
                result += $"Размер файла: {projectsJson.Length} байт\n";
            }
            else
            {
                result += $"Файл проектов: <color=red>НЕ НАЙДЕН</color>\n";
            }
            
            // Выводим путь сохранения
            result += $"\nПуть сохранения данных:\n{Application.persistentDataPath}";
            
            // Отображаем результат
            testResultText.text = result;
            Debug.Log(result.Replace("<color=green>", "").Replace("</color>", "")
                .Replace("<color=red>", "").Replace("</color>", "")
                .Replace("<b>", "").Replace("</b>", ""));
        }
    }
} 