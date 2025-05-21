using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Orion.Backend.Services;
using Orion.Backend.Models;
using System;
using System.Threading.Tasks;

namespace Orion.Tests
{
    public class RegistrationForm : MonoBehaviour
    {
        [Header("Поля ввода для пользователя")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        
        [Header("Поля ввода для организации")]
        [SerializeField] private TMP_InputField orgNameInput;
        [SerializeField] private TMP_InputField orgDescriptionInput;
        
        [Header("Кнопки")]
        [SerializeField] private Button registerButton;
        [SerializeField] private Button togglePasswordButton;
        
        [Header("Иконки для кнопки видимости пароля")]
        [SerializeField] private Sprite showPasswordIcon;
        [SerializeField] private Sprite hidePasswordIcon;
        
        [Header("Текст статуса")]
        [SerializeField] private TextMeshProUGUI statusText;
        
        private Image togglePasswordImage;
        private bool isPasswordVisible = false;
        private LoginUIManager uiManager;
        
        private void Awake()
        {
            // Получаем компонент UI Manager
            uiManager = FindFirstObjectByType<LoginUIManager>();
            
            // Настройка кнопки показа/скрытия пароля
            if (togglePasswordButton != null)
            {
                togglePasswordImage = togglePasswordButton.GetComponent<Image>();
                togglePasswordButton.onClick.AddListener(TogglePasswordVisibility);
                SetPasswordVisibility(false);
            }
            
            // Настройка кнопки регистрации
            if (registerButton != null)
            {
                registerButton.onClick.AddListener(HandleRegistration);
            }
        }
        
        // Управление видимостью пароля
        private void TogglePasswordVisibility()
        {
            SetPasswordVisibility(!isPasswordVisible);
        }
        
        private void SetPasswordVisibility(bool visible)
        {
            isPasswordVisible = visible;
            
            if (passwordInput != null)
            {
                passwordInput.contentType = visible 
                    ? TMP_InputField.ContentType.Standard 
                    : TMP_InputField.ContentType.Password;
                passwordInput.ForceLabelUpdate();
            }
            
            // Обновление иконки
            if (togglePasswordImage != null)
            {
                togglePasswordImage.sprite = visible ? hidePasswordIcon : showPasswordIcon;
            }
        }
        
        // Обработка регистрации
        public async void HandleRegistration()
        {
            // Деактивация UI для предотвращения повторных нажатий
            SetUIInteractive(false);
            
            // Проверка заполнения обязательных полей
            if (string.IsNullOrEmpty(usernameInput.text) || 
                string.IsNullOrEmpty(emailInput.text) || 
                string.IsNullOrEmpty(passwordInput.text) ||
                string.IsNullOrEmpty(orgNameInput.text))
            {
                ShowResult("Пожалуйста, заполните все обязательные поля", false);
                SetUIInteractive(true);
                return;
            }
            
            try
            {
                // Создание пользователя
                var result = await UserManager.Instance.RegisterUserAsync(
                    usernameInput.text,
                    emailInput.text,
                    passwordInput.text);
                
                if (result)
                {
                    // Создание организации
                    await CreateOrganization();
                    ShowResult("Регистрация успешна! Организация создана.", true);
                    
                    // Выполняем вход для нового пользователя
                    bool loginResult = await UserManager.Instance.LoginUserAsync(
                        emailInput.text, 
                        passwordInput.text);
                        
                    if (loginResult && uiManager != null)
                    {
                        uiManager.ShowDashboardPanel();
                    }
                }
                else
                {
                    ShowResult("Ошибка при регистрации пользователя. Возможно, пользователь с такой почтой уже существует.", false);
                }
            }
            catch (System.Exception ex)
            {
                ShowResult($"Ошибка: {ex.Message}", false);
                Debug.LogError($"Ошибка при регистрации: {ex}");
            }
            finally
            {
                SetUIInteractive(true);
            }
        }
        
        // Создание организации
        private async Task CreateOrganization()
        {
            if (UserManager.Instance.CurrentUser == null)
            {
                throw new System.Exception("Пользователь не авторизован");
            }
            
            string orgName = orgNameInput.text;
            string orgDescription = orgDescriptionInput.text;
            
            // Создаем организацию
            var result = await OrganizationManager.Instance.CreateOrganizationAsync(
                orgName,
                orgDescription,
                UserManager.Instance.CurrentUser.Id);
                
            if (result.success)
            {
                Debug.Log($"Организация '{orgName}' успешно создана");
            }
            else
            {
                throw new System.Exception($"Ошибка создания организации: {result.message}");
            }
        }
        
        // Отображение результата операции
        private void ShowResult(string message, bool isSuccess)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isSuccess ? Color.green : Color.red;
            }
        }
        
        // Управление интерактивностью UI
        private void SetUIInteractive(bool interactive)
        {
            if (usernameInput != null) usernameInput.interactable = interactive;
            if (emailInput != null) emailInput.interactable = interactive;
            if (passwordInput != null) passwordInput.interactable = interactive;
            if (orgNameInput != null) orgNameInput.interactable = interactive;
            if (orgDescriptionInput != null) orgDescriptionInput.interactable = interactive;
            if (registerButton != null) registerButton.interactable = interactive;
        }
        
        // Очистка полей формы
        public void ClearForm()
        {
            if (usernameInput != null) usernameInput.text = "";
            if (emailInput != null) emailInput.text = "";
            if (passwordInput != null) passwordInput.text = "";
            if (orgNameInput != null) orgNameInput.text = "";
            if (orgDescriptionInput != null) orgDescriptionInput.text = "";
            if (statusText != null) statusText.text = "";
            
            // Сбрасываем видимость пароля
            SetPasswordVisibility(false);
        }
    }
} 