using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Orion.Backend.Models;
using Orion.Backend.Services;

namespace Orion.Backend.UI
{
    public class AuthUIManager : MonoBehaviour
    {
        [Header("Login Panel")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button goToRegisterButton;
        [SerializeField] private TextMeshProUGUI loginErrorText;

        [Header("Register Panel")]
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private TMP_InputField registerUsernameInput;
        [SerializeField] private TMP_InputField registerEmailInput;
        [SerializeField] private TMP_InputField registerPasswordInput;
        [SerializeField] private TMP_Dropdown roleDropdown;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button goToLoginButton;
        [SerializeField] private TextMeshProUGUI registerErrorText;

        [Header("Loading")]
        [SerializeField] private GameObject loadingIndicator;

        private void Start()
        {
            // Инициализация UI элементов
            InitializeUI();
            
            // Показываем первоначальную панель (логин)
            ShowLoginPanel();
        }

        private void InitializeUI()
        {
            // Настройка обработчиков событий для кнопок
            loginButton.onClick.AddListener(OnLoginButtonClicked);
            goToRegisterButton.onClick.AddListener(() => ShowRegisterPanel());
            
            registerButton.onClick.AddListener(OnRegisterButtonClicked);
            goToLoginButton.onClick.AddListener(() => ShowLoginPanel());
            
            // Инициализация дропдауна ролей
            roleDropdown.ClearOptions();
            roleDropdown.AddOptions(new System.Collections.Generic.List<string> {
                "Пользователь", "Разработчик", "Менеджер", "Администратор"
            });
            
            // Скрытие текстов ошибок
            loginErrorText.gameObject.SetActive(false);
            registerErrorText.gameObject.SetActive(false);
            
            // Настройка ввода пароля
            loginPasswordInput.contentType = TMP_InputField.ContentType.Password;
            registerPasswordInput.contentType = TMP_InputField.ContentType.Password;
            loginPasswordInput.ForceLabelUpdate();
            registerPasswordInput.ForceLabelUpdate();
        }

        private void ShowLoginPanel()
        {
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
            ClearLoginInputs();
            loginErrorText.gameObject.SetActive(false);
        }

        private void ShowRegisterPanel()
        {
            loginPanel.SetActive(false);
            registerPanel.SetActive(true);
            ClearRegisterInputs();
            registerErrorText.gameObject.SetActive(false);
        }

        private void ClearLoginInputs()
        {
            loginEmailInput.text = "";
            loginPasswordInput.text = "";
        }

        private void ClearRegisterInputs()
        {
            registerUsernameInput.text = "";
            registerEmailInput.text = "";
            registerPasswordInput.text = "";
            roleDropdown.value = 0;
        }

        private async void OnLoginButtonClicked()
        {
            string email = loginEmailInput.text.Trim();
            string password = loginPasswordInput.text;
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowLoginError("Пожалуйста, заполните все поля");
                return;
            }
            
            SetLoadingState(true);
            
            try
            {
                var result = await UserManager.Instance.Login(email, password);
                if (result.success)
                {
                    // Успешный вход, переход к основному интерфейсу приложения
                    ShowLoginError(""); // Очистка ошибок
                    OnLoginSuccess();
                }
                else
                {
                    ShowLoginError(result.message);
                }
            }
            catch (Exception ex)
            {
                ShowLoginError($"Ошибка при входе: {ex.Message}");
                Debug.LogError($"Login error: {ex}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async void OnRegisterButtonClicked()
        {
            string username = registerUsernameInput.text.Trim();
            string email = registerEmailInput.text.Trim();
            string password = registerPasswordInput.text;
            UserRole role = GetSelectedRole();
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowRegisterError("Пожалуйста, заполните все поля");
                return;
            }
            
            if (password.Length < 6)
            {
                ShowRegisterError("Пароль должен содержать минимум 6 символов");
                return;
            }
            
            SetLoadingState(true);
            
            try
            {
                var result = await UserManager.Instance.CreateUserAsync(username, email, password, role);
                if (result.success)
                {
                    // Успешная регистрация
                    ShowRegisterError(""); // Очистка ошибок
                    
                    // Автоматический вход после регистрации
                    await UserManager.Instance.Login(email, password);
                    OnRegisterSuccess();
                }
                else
                {
                    ShowRegisterError(result.message);
                }
            }
            catch (Exception ex)
            {
                ShowRegisterError($"Ошибка при регистрации: {ex.Message}");
                Debug.LogError($"Registration error: {ex}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private UserRole GetSelectedRole()
        {
            switch (roleDropdown.value)
            {
                case 0: return UserRole.User;
                case 1: return UserRole.Developer;
                case 2: return UserRole.Manager;
                case 3: return UserRole.Admin;
                default: return UserRole.User;
            }
        }

        private void ShowLoginError(string message)
        {
            loginErrorText.text = message;
            loginErrorText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        private void ShowRegisterError(string message)
        {
            registerErrorText.text = message;
            registerErrorText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        private void SetLoadingState(bool isLoading)
        {
            loadingIndicator.SetActive(isLoading);
            loginButton.interactable = !isLoading;
            goToRegisterButton.interactable = !isLoading;
            registerButton.interactable = !isLoading;
            goToLoginButton.interactable = !isLoading;
        }

        private void OnLoginSuccess()
        {
            // Здесь нужно скрыть панель авторизации и показать основной интерфейс
            // Например, переход к панели проектов или организаций
            Debug.Log("Login successful! Redirecting to main interface...");
            gameObject.SetActive(false);
            
            // Вызов UI менеджера проектов или организаций
            // ProjectUIManager.Instance.ShowProjects();
        }

        private void OnRegisterSuccess()
        {
            Debug.Log("Registration successful! Redirecting to main interface...");
            gameObject.SetActive(false);
            
            // Вызов UI менеджера проектов или организаций
            // ProjectUIManager.Instance.ShowProjects();
            
            // Или, если нужно сначала создать/присоединиться к организации
            // OrganizationUIManager.Instance.ShowOrganizationSetup();
        }
    }
} 