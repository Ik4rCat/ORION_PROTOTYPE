using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using Orion.Backend.Services;
using Orion.Backend.Models;
using System.IO;

namespace Orion.Tests
{
    public class RegisterForm : MonoBehaviour
    {
        [Header("Поля ввода")]
        [SerializeField] private TMP_InputField usernameField;
        [SerializeField] private TMP_InputField emailField;
        [SerializeField] private TMP_InputField passwordField;
        [SerializeField] private TMP_InputField confirmPasswordField;
        [SerializeField] private TMP_InputField organizationNameField;
        [SerializeField] private TMP_InputField organizationDescField;
        [SerializeField] private TMP_Dropdown roleDropdown;
        
        [Header("Кнопки")]
        [SerializeField] private Button registerButton;
        [SerializeField] private Button showPasswordButton;
        [SerializeField] private Button showConfirmPasswordButton;
        [SerializeField] private Button backToLoginButton;
        [SerializeField] private Button continueButton;
        
        [Header("Иконки для кнопки отображения пароля")]
        [SerializeField] private Sprite showPasswordIcon;
        [SerializeField] private Sprite hidePasswordIcon;
        
        [Header("Статус")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color successColor = Color.green;
        
        private bool isPasswordVisible = false;
        private bool isConfirmPasswordVisible = false;
        
        private LoginUIManager uiManager;
        
        private void Awake()
        {
            uiManager = FindFirstObjectByType<LoginUIManager>();
            
            // Настраиваем слушатели событий
            registerButton.onClick.AddListener(Register);
            backToLoginButton.onClick.AddListener(() => uiManager.ShowLoginPanel());
            
            // Настройка кнопки продолжения
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(() => {
                    ClearForm();
                    uiManager.ShowLoginPanel();
                });
                continueButton.gameObject.SetActive(false);
            }
            
            // Настраиваем переключатели видимости пароля
            if (showPasswordButton != null)
            {
                showPasswordButton.onClick.AddListener(TogglePasswordVisibility);
                UpdatePasswordToggleIcon(showPasswordButton.GetComponent<Image>(), isPasswordVisible);
            }
            
            if (showConfirmPasswordButton != null)
            {
                showConfirmPasswordButton.onClick.AddListener(ToggleConfirmPasswordVisibility);
                UpdatePasswordToggleIcon(showConfirmPasswordButton.GetComponent<Image>(), isConfirmPasswordVisible);
            }
            
            // Настраиваем поля для пароля
            if (passwordField != null)
                passwordField.contentType = TMP_InputField.ContentType.Password;
            
            if (confirmPasswordField != null)
                confirmPasswordField.contentType = TMP_InputField.ContentType.Password;
            
            // Сбрасываем статус
            ClearStatus();
            
            // Выводим путь сохранения для отладки
            Debug.Log($"Данные сохраняются в: {Application.persistentDataPath}");
        }
        
        /// <summary>
        /// Очищает форму регистрации
        /// </summary>
        public void ClearForm()
        {
            usernameField.text = string.Empty;
            emailField.text = string.Empty;
            passwordField.text = string.Empty;
            confirmPasswordField.text = string.Empty;
            organizationNameField.text = string.Empty;
            organizationDescField.text = string.Empty;
            roleDropdown.value = 0;
            ClearStatus();
        }
        
        /// <summary>
        /// Очищает текст статуса
        /// </summary>
        private void ClearStatus()
        {
            statusText.text = string.Empty;
            statusText.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Отображает сообщение об ошибке
        /// </summary>
        private void ShowError(string message)
        {
            statusText.color = errorColor;
            statusText.text = message;
            statusText.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Отображает сообщение об успехе
        /// </summary>
        private void ShowSuccess(string message)
        {
            statusText.color = successColor;
            statusText.text = message;
            statusText.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Переключает видимость основного пароля
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
        /// Переключает видимость подтверждения пароля
        /// </summary>
        private void ToggleConfirmPasswordVisibility()
        {
            if (confirmPasswordField == null || showConfirmPasswordButton == null) return;
            
            isConfirmPasswordVisible = !isConfirmPasswordVisible;
            confirmPasswordField.contentType = isConfirmPasswordVisible ? TMP_InputField.ContentType.Standard 
                                                                     : TMP_InputField.ContentType.Password;
            confirmPasswordField.ForceLabelUpdate();
            
            UpdatePasswordToggleIcon(showConfirmPasswordButton.GetComponent<Image>(), isConfirmPasswordVisible);
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
        /// Проверяет валидность полей формы
        /// </summary>
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(usernameField.text))
            {
                ShowError("Введите имя пользователя");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(emailField.text) || !emailField.text.Contains("@"))
            {
                ShowError("Введите корректный email адрес");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(passwordField.text) || passwordField.text.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов");
                return false;
            }
            
            if (passwordField.text != confirmPasswordField.text)
            {
                ShowError("Пароли не совпадают");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(organizationNameField.text))
            {
                ShowError("Введите название организации");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Обработчик нажатия на кнопку регистрации
        /// </summary>
        private async void Register()
        {
            if (!ValidateForm())
            {
                return;
            }
            
            // Отключаем кнопку на время регистрации
            registerButton.interactable = false;
            
            try
            {
                // Определяем роль пользователя
                UserRole role = UserRole.Developer; // Для тестов регистрируем только разработчиков
                
                // Регистрируем пользователя
                User newUser = await RegisterUserAsync(
                    usernameField.text,
                    emailField.text,
                    passwordField.text,
                    role
                );
                
                if (newUser != null)
                {
                    // Создаем организацию для разработчика
                    await CreateOrganizationAsync(organizationNameField.text, newUser.Id);
                    
                    ShowSuccess("Регистрация успешна! Выполняется вход...");
                    
                    // Выполняем автоматический вход
                    bool loginResult = await UserManager.Instance.LoginUserAsync(
                        emailField.text, 
                        passwordField.text
                    );
                    
                    if (loginResult)
                    {
                        // Активируем кнопку продолжения
                        if (continueButton != null)
                        {
                            continueButton.gameObject.SetActive(true);
                            registerButton.gameObject.SetActive(false);
                            backToLoginButton.gameObject.SetActive(false);
                        }
                        
                        // Задержка перед переходом на дашборд
                        await Task.Delay(1500);
                        
                        // Переходим сразу на дашборд
                        uiManager.ShowDashboardPanel();
                    }
                    else
                    {
                        ShowError("Ошибка автоматического входа. Пожалуйста, войдите вручную.");
                        await Task.Delay(2000);
                        ClearForm();
                        uiManager.ShowLoginPanel();
                    }
                }
                else
                {
                    ShowError("Ошибка при регистрации пользователя");
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
                Debug.LogError($"Ошибка регистрации: {ex}");
            }
            finally
            {
                // Включаем кнопку обратно
                registerButton.interactable = true;
            }
        }
        
        /// <summary>
        /// Асинхронно регистрирует пользователя
        /// </summary>
        private async Task<User> RegisterUserAsync(string username, string email, string password, UserRole role)
        {
            try
            {
                // Проверяем, существует ли пользователь с таким email
                if (await UserManager.Instance.UserExistsByEmailAsync(email))
                {
                    ShowError("Пользователь с таким email уже существует");
                    return null;
                }
                
                // Регистрируем пользователя
                var success = await UserManager.Instance.RegisterUserAsync(username, email, password, role);
                if (success)
                {
                    // Если регистрация прошла успешно, получаем пользователя по email
                    return UserManager.Instance.GetUserByEmail(email);
                }
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Ошибка при регистрации пользователя: {ex}");
                throw;
            }
        }
        
        /// <summary>
        /// Асинхронно создает организацию для разработчика
        /// </summary>
        private async Task<Organization> CreateOrganizationAsync(string organizationName, string ownerId)
        {
            try
            {
                // Создаем организацию
                var result = await OrganizationManager.Instance.CreateOrganizationAsync(
                    organizationName,
                    $"Организация разработчика {organizationName}",
                    ownerId
                );
                
                if (result.success)
                {
                    return result.createdOrg;
                }
                else
                {
                    throw new System.Exception(result.message);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Ошибка при создании организации: {ex}");
                throw;
            }
        }
    }
} 