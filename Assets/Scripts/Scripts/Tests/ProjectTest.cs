using UnityEngine;
using TMPro;
using Orion.Backend.Services;
using Orion.Backend.Models;
using System.Threading.Tasks;

namespace Orion.Tests
{
    public class ProjectTest : MonoBehaviour
    {
        [Header("Организация")]
        [SerializeField] private TMP_InputField orgNameInput;
        [SerializeField] private TMP_InputField orgDescInput;
        [SerializeField] private TextMeshProUGUI orgStatusText;
        
        [Header("Проект")]
        [SerializeField] private TMP_InputField projectNameInput;
        [SerializeField] private TMP_InputField projectDescInput;
        [SerializeField] private TextMeshProUGUI projectStatusText;
        [SerializeField] private TextMeshProUGUI projectListText;
        
        // Создание организации
        public async void TestCreateOrganization()
        {
            var user = UserManager.Instance.CurrentUser;
            if (user == null)
            {
                orgStatusText.text = "Требуется авторизация";
                Debug.LogWarning("Невозможно создать организацию: пользователь не авторизован");
                return;
            }
            
            string name = orgNameInput.text;
            string desc = orgDescInput.text;
            
            if (string.IsNullOrEmpty(name))
            {
                orgStatusText.text = "Укажите название организации";
                return;
            }
            
            var result = await OrganizationManager.Instance.CreateOrganizationAsync(
                name, desc, user.Id);
            
            orgStatusText.text = result.success ? 
                $"Организация '{name}' успешно создана" : 
                $"Ошибка: {result.message}";
                
            if (result.success)
            {
                Debug.Log($"Создана организация: {name}");
            }
        }
        
        // Создание проекта
        public async void TestCreateProject()
        {
            var user = UserManager.Instance.CurrentUser;
            if (user == null)
            {
                projectStatusText.text = "Требуется авторизация";
                Debug.LogWarning("Невозможно создать проект: пользователь не авторизован");
                return;
            }
            
            if (string.IsNullOrEmpty(user.OrganizationId))
            {
                projectStatusText.text = "Сначала нужно создать или присоединиться к организации";
                return;
            }
            
            string name = projectNameInput.text;
            string desc = projectDescInput.text;
            
            if (string.IsNullOrEmpty(name))
            {
                projectStatusText.text = "Укажите название проекта";
                return;
            }
            
            var result = await ProjectManager.Instance.CreateProjectAsync(
                user.OrganizationId, name, desc);
            
            projectStatusText.text = result.success ? 
                $"Проект '{name}' успешно создан" : 
                $"Ошибка: {result.message}";
                
            if (result.success)
            {
                Debug.Log($"Создан проект: {name} в организации {user.OrganizationId}");
                await ListProjects();
            }
        }
        
        // Получение списка проектов
        public async Task ListProjects()
        {
            var user = UserManager.Instance.CurrentUser;
            if (user == null || string.IsNullOrEmpty(user.OrganizationId))
            {
                projectListText.text = "Нет доступных проектов";
                return;
            }
            
            // Добавляем фиктивную асинхронную операцию для исправления предупреждения
            await Task.Yield();
            
            var projects = ProjectManager.Instance.GetProjectsByOrganization(user.OrganizationId);
            
            if (projects.Count == 0)
            {
                projectListText.text = "Проекты не найдены";
                return;
            }
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Проекты:");
            
            foreach (var project in projects)
            {
                sb.AppendLine($"- {project.Name}: {project.Description}");
            }
            
            projectListText.text = sb.ToString();
        }
    }
} 