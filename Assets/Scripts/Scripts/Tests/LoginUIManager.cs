using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Orion.Backend.Services;
using Orion.Backend.Models;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Orion.Tests
{
    public class LoginUIManager : MonoBehaviour
    {
        [Header("Панели")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private GameObject dashboardPanel;
        [SerializeField] private GameObject loadingPanel;
        
        [Header("Переключатели панелей")]
        [SerializeField] private Button toRegisterButton;
        [SerializeField] private Button toLoginButton;
        
        [Header("Текст статуса")]
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Скрипты форм")]
        [SerializeField] private LoginForm loginForm;
        [SerializeField] private RegisterForm registerForm;
        
        [Header("Информация о пользователе")]
        [SerializeField] private TextMeshProUGUI userInfoText;
        [SerializeField] private Button logoutButton;
        
        [Header("Тесты")]
        [SerializeField] private AuthTest authTest;
        
        [Header("Панели и их компоненты")]
        [SerializeField] private DashboardPanel dashboardPanelComponent;
        
        private Dictionary<string, GameObject> panelsMap = new Dictionary<string, GameObject>();
        private string currentPanelId;
        private User _currentUser = null;
        
        private GameObject _currentActivePanel;
        private Coroutine _transitionCoroutine;
        
        private void Awake()
        {
            // Убедимся, что есть ссылка на тест авторизации
            if (authTest == null)
            {
                authTest = FindFirstObjectByType<AuthTest>();
            }
            
            // Скрываем все панели при инициализации
            if (loginPanel != null) loginPanel.SetActive(false);
            if (registerPanel != null) registerPanel.SetActive(false);
            if (dashboardPanel != null) dashboardPanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            
            // Устанавливаем панель входа как активную по умолчанию
            _currentActivePanel = loginPanel;
            
            // Проверяем наличие всех необходимых панелей
            if (loginPanel == null || registerPanel == null || dashboardPanel == null)
            {
                Debug.LogError("Не все панели назначены в LoginUIManager!");
            }
            
            // Проверяем наличие скриптов форм
            if (loginForm == null || registerForm == null)
            {
                Debug.LogError("Скрипты форм не назначены в LoginUIManager!");
            }
        }
        
        private void Start()
        {
            // Проверяем, существуют ли все необходимые менеджеры
            if (UserManager.Instance == null)
            {
                Debug.LogError("UserManager не найден на сцене!");
                return;
            }
            
            // Подписываемся на события изменения состояния пользователя
            UserManager.Instance.OnUserCreated += OnUserStateChanged;
            UserManager.Instance.OnUserLoggedIn += OnUserStateChanged;
            UserManager.Instance.OnUserLoggedOut += OnUserStateChanged;
            
            // Настраиваем кнопки
            logoutButton.onClick.AddListener(LogoutUser);
            
            // Проверяем, вошел ли пользователь в систему
            _currentUser = UserManager.Instance.CurrentUser;
            UpdateUIState();
            
            Debug.Log("LoginUIManager инициализирован");
            
            // Настройка обработчиков кнопок переключения панелей
            if (toRegisterButton != null)
                toRegisterButton.onClick.AddListener(ShowRegisterPanel);
                
            if (toLoginButton != null)
                toLoginButton.onClick.AddListener(ShowLoginPanel);
            
            // Показываем начальную панель
            ShowLoginPanel();
        }
        
        private void OnDestroy()
        {
            // Отписываемся от событий
            if (UserManager.Instance != null)
            {
                UserManager.Instance.OnUserCreated -= OnUserStateChanged;
                UserManager.Instance.OnUserLoggedIn -= OnUserStateChanged;
                UserManager.Instance.OnUserLoggedOut -= OnUserStateChanged;
            }
        }
        
        private void OnUserStateChanged(User user)
        {
            _currentUser = user;
            UpdateUIState();
        }
        
        private void LogoutUser()
        {
            UserManager.Instance.Logout();
        }
        
        private void UpdateUIState()
        {
            if (_currentUser != null)
            {
                // Пользователь вошел в систему
                ShowDashboardPanel();
                
                // Отображаем информацию о пользователе
                userInfoText.text = $"Пользователь: {_currentUser.Username}\nEmail: {_currentUser.Email}\nРоль: {_currentUser.Role}";
                
                // Обновляем компонент DashboardPanel, если он есть
                if (dashboardPanelComponent != null)
                {
                    dashboardPanelComponent.UpdateUserInfo();
                }
            }
            else
            {
                // Пользователь не вошел в систему
                ShowLoginPanel();
                
                userInfoText.text = "Не авторизован";
            }
        }
        
        /// <summary>
        /// Показывает панель логина
        /// </summary>
        public void ShowLoginPanel()
        {
            SwitchPanel(loginPanel);
            
            // Очищаем форму при возвращении к экрану входа
            if (authTest != null)
            {
                authTest.ClearForm();
            }
        }
        
        /// <summary>
        /// Показывает панель регистрации
        /// </summary>
        public void ShowRegisterPanel()
        {
            SwitchPanel(registerPanel);
        }
        
        /// <summary>
        /// Показывает панель дашборда
        /// </summary>
        public void ShowDashboardPanel()
        {
            SwitchPanel(dashboardPanel);
        }
        
        /// <summary>
        /// Показывает анимацию загрузки
        /// </summary>
        public void ShowLoadingPanel()
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// Скрывает анимацию загрузки
        /// </summary>
        public void HideLoadingPanel()
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Переключение между панелями с анимацией
        /// </summary>
        private void SwitchPanel(GameObject newPanel)
        {
            if (newPanel == null || newPanel == _currentActivePanel)
            {
                return;
            }
            
            // Останавливаем предыдущую анимацию, если она выполняется
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            
            _transitionCoroutine = StartCoroutine(TransitionPanels(_currentActivePanel, newPanel));
            _currentActivePanel = newPanel;
        }
        
        /// <summary>
        /// Анимация перехода между панелями
        /// </summary>
        private IEnumerator TransitionPanels(GameObject currentPanel, GameObject nextPanel)
        {
            // Показываем загрузку во время перехода
            ShowLoadingPanel();
            
            // Скрываем текущую панель
            if (currentPanel != null)
            {
                currentPanel.SetActive(false);
            }
            
            // Задержка для эффекта загрузки
            yield return new WaitForSeconds(0.5f);
            
            // Скрываем загрузку
            HideLoadingPanel();
            
            // Показываем новую панель
            if (nextPanel != null)
            {
                nextPanel.SetActive(true);
            }
            
            _transitionCoroutine = null;
        }
        
        /// <summary>
        /// Возвращает текущий активный ID панели
        /// </summary>
        public string GetCurrentPanelId()
        {
            return currentPanelId;
        }
    }
} 