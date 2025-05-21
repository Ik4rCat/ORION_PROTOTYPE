using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using Orion.Backend.Services;

namespace Orion.Tests
{
    public class LoginForm : MonoBehaviour
    {
        [Header("Поля ввода")]
        [SerializeField] private TMP_InputField emailField;
        [SerializeField] private TMP_InputField passwordField;
        
        [Header("Кнопки")]
        [SerializeField] private Button loginButton;
        [SerializeField] private Button showPasswordButton;
        [SerializeField] private Button registerButton;
        
        [Header("Иконки для кнопки отображения пароля")]
        [SerializeField] private Sprite showPasswordIcon;
        [SerializeField] private Sprite hidePasswordIcon;
        
        [Header("UI элементы статуса")]
        [SerializeField] private TextMeshProUGUI statusText;
        
        private bool isPasswordVisible = false;
        
        private void Start()
        {
            // Инициализация кнопок
            if (loginButton != null)
                loginButton.onClick.AddListener(Login);
                
            if (registerButton != null && FindFirstObjectByType<LoginUIManager>() != null)
                registerButton.onClick.AddListener(FindFirstObjectByType<LoginUIManager>().ShowRegisterPanel);
                
            // Настройка кнопки видимости пароля
            if (showPasswordButton != null)
            {
                showPasswordButton.onClick.AddListener(TogglePasswordVisibility);
                UpdatePasswordToggleIcon(showPasswordButton.GetComponent<Image>(), isPasswordVisible);
            }
            
            // Инициализация поля пароля
            if (passwordField != null)
                passwordField.contentType = TMP_InputField.ContentType.Password;
                
            ClearForm();
        }
        
        /// <summary>
        /// Очищает все поля формы и статусный текст
        /// </summary>
        public void ClearForm()
        {
            if (emailField != null) emailField.text = string.Empty;
            if (passwordField != null) passwordField.text = string.Empty;
            
            if (statusText != null) statusText.text = string.Empty;
        }
        
        /// <summary>
        /// Переключает видимость пароля
        /// </summary>
        private void TogglePasswordVisibility()
        {
            if (passwordField == null || showPasswordButton == null) return;
            
            isPasswordVisible = !isPasswordVisible;
            passwordField.contentType = isPasswordVisible ? TMP_InputField.ContentType.Standard 
                                                       : TMP_InputField.ContentType.Password;
            passwordField.ForceLabelUpdate();
            
            UpdatePasswordToggleIcon(showPasswordButton.GetComponent<Image>(), isPasswordVisible);
        }
        
        /// <summary>
        /// Обновляет иконку на кнопке отображения пароля
        /// </summary>
        private void UpdatePasswordToggleIcon(Image buttonImage, bool isVisible)
        {
            if (buttonImage != null)
            {
                buttonImage.sprite = isVisible ? hidePasswordIcon : showPasswordIcon;
            }
        }
        
        /// <summary>
        /// Отображает статусное сообщение
        /// </summary>
        private void ShowStatus(string message, bool isError = false)
        {
            if (statusText == null) return;
            
            statusText.text = message;
            statusText.color = isError ? Color.red : Color.green;
        }
        
        /// <summary>
        /// Проверяет валидность полей формы
        /// </summary>
        private bool ValidateForm()
        {
            // Проверка заполнения полей
            if (string.IsNullOrWhiteSpace(emailField.text))
            {
                ShowStatus("Необходимо указать email", true);
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(passwordField.text))
            {
                ShowStatus("Необходимо указать пароль", true);
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Обрабатывает нажатие на кнопку входа
        /// </summary>
        public void Login()
        {
            if (!ValidateForm()) return;
            
            // Отключаем кнопку на время авторизации
            if (loginButton != null)
                loginButton.interactable = false;
                
            ShowStatus("Выполняется вход...");
            
            // Вызываем асинхронный метод авторизации
            _ = LoginAsync();
        }
        
        /// <summary>
        /// Выполняет асинхронную авторизацию пользователя
        /// </summary>
        private async Task LoginAsync()
        {
            try
            {
                var userManager = FindFirstObjectByType<Backend.Services.UserManager>();
                if (userManager == null)
                {
                    ShowStatus("Ошибка: UserManager не найден", true);
                    loginButton.interactable = true;
                    return;
                }
                
                // Выполняем вход
                bool success = await userManager.LoginUserAsync(
                    emailField.text,
                    passwordField.text
                );
                
                if (!success)
                {
                    ShowStatus("Неверный email или пароль", true);
                    loginButton.interactable = true;
                    return;
                }
                
                ShowStatus("Вход выполнен успешно!");
                
                // Переходим в основное приложение после короткой паузы
                await Task.Delay(1000);
                FindFirstObjectByType<LoginUIManager>().ShowDashboardPanel();
            }
            catch (System.Exception ex)
            {
                ShowStatus($"Ошибка: {ex.Message}", true);
                Debug.LogError($"Ошибка при входе: {ex}");
            }
            finally
            {
                // Возвращаем кнопке интерактивность
                if (loginButton != null)
                    loginButton.interactable = true;
            }
        }
    }
} 