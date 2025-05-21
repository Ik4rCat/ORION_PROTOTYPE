using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Orion.Backend.Services;

namespace Orion.Tests
{
    public class AuthTest : MonoBehaviour
    {
        [Header("Поля ввода")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        
        [Header("Кнопки")]
        [SerializeField] private Button loginButton;
        [SerializeField] private Button logoutButton;
        [SerializeField] private Button switchToRegisterButton;
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
            
            // Настройка кнопок
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(HandleLogin);
            }
            
            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(HandleLogout);
                // Скрываем кнопку выхода изначально
                logoutButton.gameObject.SetActive(false);
            }
            
            if (switchToRegisterButton != null && uiManager != null)
            {
                switchToRegisterButton.onClick.AddListener(uiManager.ShowRegisterPanel);
            }
        }
        
        private void Start()
        {
            // Проверяем статус авторизации при старте
            UpdateUIBasedOnAuthStatus();
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
        
        // Обработка входа
        public async void HandleLogin()
        {
            // Деактивация UI для предотвращения повторных нажатий
            SetUIInteractive(false);
            
            // Проверка заполнения обязательных полей
            if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text))
            {
                ShowResult("Пожалуйста, введите email и пароль", false);
                SetUIInteractive(true);
                return;
            }
            
            try
            {
                ShowResult("Выполняется вход...", null);
                
                bool result = await UserManager.Instance.LoginUserAsync(
                    emailInput.text, 
                    passwordInput.text);
                
                if (result)
                {
                    ShowResult("Вход выполнен успешно!", true);
                    StartCoroutine(ShowDashboardAfterDelay(1.0f));
                }
                else
                {
                    ShowResult("Неверный email или пароль", false);
                }
            }
            catch (System.Exception ex)
            {
                ShowResult($"Ошибка: {ex.Message}", false);
                Debug.LogError($"Ошибка при входе: {ex}");
            }
            finally
            {
                SetUIInteractive(true);
                UpdateUIBasedOnAuthStatus();
            }
        }
        
        // Обработка выхода
        public void HandleLogout()
        {
            try
            {
                UserManager.Instance.LogOut();
                ShowResult("Выход выполнен", true);
                
                // Очищаем поля ввода
                ClearForm();
            }
            catch (System.Exception ex)
            {
                ShowResult($"Ошибка при выходе: {ex.Message}", false);
                Debug.LogError($"Ошибка при выходе: {ex}");
            }
            finally
            {
                UpdateUIBasedOnAuthStatus();
            }
        }
        
        // Обновление UI в зависимости от статуса авторизации
        private void UpdateUIBasedOnAuthStatus()
        {
            bool isLoggedIn = UserManager.Instance.IsUserLoggedIn();
            
            if (loginButton != null)
                loginButton.gameObject.SetActive(!isLoggedIn);
                
            if (logoutButton != null)
                logoutButton.gameObject.SetActive(isLoggedIn);
                
            if (emailInput != null)
                emailInput.gameObject.SetActive(!isLoggedIn);
                
            if (passwordInput != null)
                passwordInput.gameObject.SetActive(!isLoggedIn);
                
            if (switchToRegisterButton != null)
                switchToRegisterButton.gameObject.SetActive(!isLoggedIn);
                
            if (togglePasswordButton != null)
                togglePasswordButton.gameObject.SetActive(!isLoggedIn);
                
            // Если пользователь уже авторизован при запуске, показываем панель дашборда
            if (isLoggedIn && uiManager != null)
            {
                uiManager.ShowDashboardPanel();
            }
        }
        
        // Показ панели дашборда с задержкой
        private IEnumerator ShowDashboardAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            
            if (uiManager != null)
            {
                uiManager.ShowDashboardPanel();
            }
        }
        
        // Отображение результата операции
        private void ShowResult(string message, bool? isSuccess)
        {
            if (statusText != null)
            {
                statusText.text = message;
                
                if (isSuccess.HasValue)
                {
                    statusText.color = isSuccess.Value ? Color.green : Color.red;
                }
                else
                {
                    statusText.color = Color.white;
                }
            }
        }
        
        // Управление интерактивностью UI
        private void SetUIInteractive(bool interactive)
        {
            if (emailInput != null) emailInput.interactable = interactive;
            if (passwordInput != null) passwordInput.interactable = interactive;
            if (loginButton != null) loginButton.interactable = interactive;
            if (switchToRegisterButton != null) switchToRegisterButton.interactable = interactive;
        }
        
        // Очистка полей формы
        public void ClearForm()
        {
            if (emailInput != null) emailInput.text = "";
            if (passwordInput != null) passwordInput.text = "";
            if (statusText != null) statusText.text = "";
            
            // Сбрасываем видимость пароля
            SetPasswordVisibility(false);
        }
    }
} 