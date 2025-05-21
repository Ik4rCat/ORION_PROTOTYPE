using System.Collections;
using UnityEngine;
using Orion.Backend.Services;

namespace Orion.Backend.UI
{
    public class AppInitializer : MonoBehaviour
    {
        [Header("Service Prefabs")]
        [SerializeField] private GameObject userManagerPrefab;
        [SerializeField] private GameObject organizationManagerPrefab;
        [SerializeField] private GameObject projectManagerPrefab;
        [SerializeField] private GameObject taskManagerPrefab;
        [SerializeField] private GameObject commentManagerPrefab;
        [SerializeField] private GameObject activityLogManagerPrefab;
        [SerializeField] private GameObject gitIntegrationManagerPrefab;

        [Header("UI Managers")]
        [SerializeField] private GameObject authUIManagerPrefab;
        [SerializeField] private GameObject taskUIManagerPrefab;

        [Header("Initialization")]
        [SerializeField] private float initializationDelay = 0.5f;
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private GameObject mainCanvas;

        private void Start()
        {
            loadingScreen.SetActive(true);
            mainCanvas.SetActive(false);
            StartCoroutine(InitializeApp());
        }

        private IEnumerator InitializeApp()
        {
            Debug.Log("Starting application initialization...");

            // Инициализация сервисов в правильном порядке
            yield return InitializeManager(userManagerPrefab, "UserManager");
            yield return new WaitForSeconds(initializationDelay);

            yield return InitializeManager(organizationManagerPrefab, "OrganizationManager");
            yield return new WaitForSeconds(initializationDelay);

            yield return InitializeManager(projectManagerPrefab, "ProjectManager");
            yield return new WaitForSeconds(initializationDelay);

            yield return InitializeManager(taskManagerPrefab, "TaskManager");
            yield return new WaitForSeconds(initializationDelay);

            yield return InitializeManager(commentManagerPrefab, "CommentManager");
            yield return new WaitForSeconds(initializationDelay);

            yield return InitializeManager(activityLogManagerPrefab, "ActivityLogManager");
            yield return new WaitForSeconds(initializationDelay);

            yield return InitializeManager(gitIntegrationManagerPrefab, "GitIntegrationManager");
            yield return new WaitForSeconds(initializationDelay);

            // Инициализация UI менеджеров
            yield return InitializeManager(authUIManagerPrefab, "AuthUIManager");
            yield return InitializeManager(taskUIManagerPrefab, "TaskUIManager", false);

            // Завершение инициализации
            Debug.Log("Application initialization complete!");

            // Скрытие загрузочного экрана
            loadingScreen.SetActive(false);
            mainCanvas.SetActive(true);

            // Проверка авторизации и отображение соответствующего интерфейса
            if (UserManager.Instance.CurrentUser == null)
            {
                // Пользователь не авторизован - показать экран авторизации
                if (GameObject.Find("AuthUIManager") != null)
                {
                    GameObject.Find("AuthUIManager").SetActive(true);
                }
                
                if (GameObject.Find("TaskUIManager") != null)
                {
                    GameObject.Find("TaskUIManager").SetActive(false);
                }
            }
            else
            {
                // Пользователь авторизован - показать основной интерфейс
                if (GameObject.Find("AuthUIManager") != null)
                {
                    GameObject.Find("AuthUIManager").SetActive(false);
                }
                
                if (GameObject.Find("TaskUIManager") != null)
                {
                    GameObject.Find("TaskUIManager").SetActive(true);
                }
            }
        }

        private IEnumerator InitializeManager(GameObject prefab, string managerName, bool dontDestroyOnLoad = true)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab for {managerName} is not assigned!");
                yield break;
            }

            // Проверяем, существует ли уже экземпляр менеджера
            var existingManager = GameObject.Find(managerName);
            if (existingManager != null)
            {
                Debug.Log($"{managerName} already exists, skipping initialization.");
                yield break;
            }

            // Создаем экземпляр менеджера
            var managerInstance = Instantiate(prefab);
            managerInstance.name = managerName;
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(managerInstance);
            }
            
            Debug.Log($"{managerName} initialized successfully.");
            yield return null;
        }
    }
} 