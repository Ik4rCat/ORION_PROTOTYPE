using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Orion.Backend.Models;
using Orion.Backend.Services;

namespace Orion.Backend.Core
{
    public class AppManager : MonoBehaviour
    {
        [Header("Initialization")]
        [SerializeField] GameObject loadingScreen;
        [SerializeField] float startupDelay = 1.0f;

        [Header("Managers")]
        [SerializeField] GameObject userManagerPrefab;
        
        [Header("UI")]
        [SerializeField] Button loginButton;
        [SerializeField] Button registerButton;

        private static AppManager _instance;
        public static AppManager Instance => _instance;

        private void Awake()
        {
            // Синглтон
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Инициализация приложения
            StartCoroutine(InitializeApp());
        }

        private IEnumerator InitializeApp()
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(true);

            Debug.Log("Initializing application...");

            // Инициализация UserManager
            InitializeUserManager();

            // Ожидание для загрузки данных
            yield return new WaitForSeconds(startupDelay);

            // Регистрация обработчиков событий для UI
            RegisterUIEvents();

            if (loadingScreen != null)
                loadingScreen.SetActive(false);

            Debug.Log("Application initialized!");
        }

        private void InitializeUserManager()
        {
            if (userManagerPrefab != null && UserManager.Instance == null)
            {
                GameObject userManagerObj = Instantiate(userManagerPrefab);
                userManagerObj.name = "UserManager";
                DontDestroyOnLoad(userManagerObj);
            }
        }

        private void RegisterUIEvents()
        {
            if (UserManager.Instance != null)
            {
                UserManager.Instance.OnUserChanged += OnUserChanged;
            }

            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginButtonClicked);
            }

            if (registerButton != null)
            {
                registerButton.onClick.AddListener(OnRegisterButtonClicked);
            }
        }

        private void OnUserChanged(User user)
        {
            Debug.Log($"User changed: {(user != null ? user.Username : "Logged out")}");
            
            // Здесь логика обновления UI в зависимости от текущего пользователя
            // Например, показать/скрыть панели или обновить информацию пользователя
        }

        private void OnLoginButtonClicked()
        {
            // Показать панель входа
            Debug.Log("Login button clicked!");
            // TODO: Показать окно входа
        }

        private void OnRegisterButtonClicked()
        {
            // Показать панель регистрации
            Debug.Log("Register button clicked!");
            // TODO: Показать окно регистрации
        }

        private void OnDestroy()
        {
            // Отписка от событий
            if (UserManager.Instance != null)
            {
                UserManager.Instance.OnUserChanged -= OnUserChanged;
            }

            if (loginButton != null)
            {
                loginButton.onClick.RemoveListener(OnLoginButtonClicked);
            }

            if (registerButton != null)
            {
                registerButton.onClick.RemoveListener(OnRegisterButtonClicked);
            }
        }
    }
} 