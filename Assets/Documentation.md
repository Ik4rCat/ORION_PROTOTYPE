# Orion - Многофункциональная Платформа для Сотрудничества и Управления Проектами

## Обзор проекта

Orion - это настольное приложение на базе Unity и C#, которое объединяет функциональность Obsidian, Miro и YouGile (Trello) в одной платформе. Приложение предназначено для удовлетворения потребностей как обычных пользователей, так и разработчиков, с особым фокусом на разработчиков игр.

### Ключевые особенности

- **Двойная ролевая система**: Обычный пользователь и разработчик с различными уровнями доступа и функциональностью
- **Интерактивные доски Kanban**: Для управления задачами и планирования проектов
- **Гибкие холсты (Canvas)**: Для визуального мышления и коллаборативного дизайна
- **Сетевые заметки**: Для управления знаниями в стиле Obsidian
- **Многопользовательское сотрудничество**: Создание "семей" или компаний с неограниченным количеством участников
- **Инструменты для разработчиков игр**: Расширенные возможности для игровой разработки

## Архитектура проекта

### Структура папок

```
Assets/
├── Scripts/               # Скрипты C#
│   ├── Core/              # Основная логика приложения
│   ├── UI/                # Компоненты пользовательского интерфейса
│   ├── Models/            # Модели данных
│   ├── Services/          # Сервисы (сеть, хранение данных и т.д.)
│   ├── Utils/             # Вспомогательные утилиты
│   └── Extensions/        # Расширения для Unity и .NET
├── Scenes/                # Сцены Unity
├── Resources/             # Ресурсы (шрифты, иконки и т.д.)
├── Prefabs/               # Префабы компонентов
└── Plugins/               # Сторонние плагины и библиотеки
```

### Используемые библиотеки и технологии

1. **Основные библиотеки Unity**:
   - TextMeshPro для продвинутого текстового рендеринга
   - Unity UI для базовых компонентов интерфейса
   - URP (Universal Render Pipeline) для улучшенного рендеринга

2. **Библиотеки .NET и C#**:
   - System.IO для работы с файловой системой
   - System.Text.Json для обработки JSON
   - System.Threading.Tasks для асинхронных операций
   - System.Collections.Concurrent для потокобезопасных коллекций

3. **Сторонние библиотеки**:
   - Mirror или Photon для сетевого взаимодействия
   - SQLite для локального хранения данных
   - ReactiveX для реактивного программирования
   - Markdig для рендеринга Markdown

## Компоненты системы

### 1. Система аутентификации и профилей пользователей

```csharp
// UserManager.cs
public class UserManager : MonoBehaviour
{
    public enum UserRole { Standard, Developer }
    
    // Текущий авторизованный пользователь
    public static User CurrentUser { get; private set; }
    
    // Авторизация пользователя
    public async Task<bool> AuthenticateUser(string username, string password)
    {
        // Реализация аутентификации
    }
    
    // Регистрация нового пользователя
    public async Task<bool> RegisterUser(string username, string password, UserRole role)
    {
        // Реализация регистрации
    }
}

// User.cs
[Serializable]
public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public UserManager.UserRole Role { get; set; }
    public List<string> TeamIds { get; set; } = new List<string>();
    
    // Дополнительные свойства в зависимости от роли
    public bool HasDeveloperAccess => Role == UserManager.UserRole.Developer;
}
```

### 2. Система Kanban-досок

```csharp
// KanbanBoard.cs
[Serializable]
public class KanbanBoard
{
    public string Id { get; set; }
    public string Title { get; set; }
    public List<KanbanColumn> Columns { get; set; } = new List<KanbanColumn>();
    public List<string> MemberIds { get; set; } = new List<string>();
    
    // Методы для управления доской
    public void AddColumn(string title)
    {
        // Добавление колонки
    }
    
    public void MoveCard(string cardId, string sourceColumnId, string targetColumnId, int targetIndex)
    {
        // Перемещение карточки
    }
}

// KanbanColumn.cs
[Serializable]
public class KanbanColumn
{
    public string Id { get; set; }
    public string Title { get; set; }
    public List<KanbanCard> Cards { get; set; } = new List<KanbanCard>();
}

// KanbanCard.cs
[Serializable]
public class KanbanCard
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<string> AssigneeIds { get; set; } = new List<string>();
    public List<string> Tags { get; set; } = new List<string>();
    public DateTime DueDate { get; set; }
}
```

### 3. Система Canvas для визуального планирования

```csharp
// Canvas.cs
[Serializable]
public class Canvas
{
    public string Id { get; set; }
    public string Title { get; set; }
    public List<CanvasElement> Elements { get; set; } = new List<CanvasElement>();
    public Vector2 Size { get; set; } = new Vector2(5000, 5000);
    
    // Методы для управления элементами на холсте
    public void AddElement(CanvasElement element)
    {
        // Добавление элемента
    }
    
    public void RemoveElement(string elementId)
    {
        // Удаление элемента
    }
}

// CanvasElement.cs
[Serializable]
public abstract class CanvasElement
{
    public string Id { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public float Rotation { get; set; }
    public Color Color { get; set; }
    
    public abstract void Render();
}

// Примеры конкретных элементов
[Serializable]
public class TextElement : CanvasElement
{
    public string Text { get; set; }
    public float FontSize { get; set; }
    
    public override void Render()
    {
        // Рендеринг текстового элемента
    }
}

[Serializable]
public class ImageElement : CanvasElement
{
    public string ImagePath { get; set; }
    
    public override void Render()
    {
        // Рендеринг изображения
    }
}

[Serializable]
public class ConnectionElement : CanvasElement
{
    public string SourceElementId { get; set; }
    public string TargetElementId { get; set; }
    public LineType LineType { get; set; }
    
    public enum LineType { Straight, Curved, Arrow }
    
    public override void Render()
    {
        // Рендеринг линии соединения
    }
}
```

### 4. Система заметок в стиле Obsidian

```csharp
// Note.cs
[Serializable]
public class Note
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    
    // Связи с другими заметками
    public List<string> LinkedNoteIds { get; set; } = new List<string>();
    
    // Парсинг контента для извлечения ссылок на другие заметки
    public List<string> ExtractLinks()
    {
        // Логика извлечения ссылок в формате [[Название заметки]]
    }
}

// NoteManager.cs
public class NoteManager : MonoBehaviour
{
    private Dictionary<string, Note> _notesById = new Dictionary<string, Note>();
    
    // Загрузка всех заметок
    public async Task LoadNotes()
    {
        // Загрузка заметок из хранилища
    }
    
    // Создание новой заметки
    public Note CreateNote(string title)
    {
        // Создание заметки
    }
    
    // Получение графа связей между заметками
    public Dictionary<string, List<string>> GetNoteGraph()
    {
        // Формирование графа связей
    }
}
```

### 5. Система "Семей" (Команд)

```csharp
// Team.cs
[Serializable]
public class Team
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string OwnerId { get; set; }
    public List<TeamMember> Members { get; set; } = new List<TeamMember>();
    public List<string> BoardIds { get; set; } = new List<string>();
    public List<string> CanvasIds { get; set; } = new List<string>();
    public List<string> NoteIds { get; set; } = new List<string>();
    
    // Методы управления командой
    public bool AddMember(string userId, TeamRole role)
    {
        // Добавление участника
    }
    
    public bool RemoveMember(string userId)
    {
        // Удаление участника
    }
}

// TeamMember.cs
[Serializable]
public class TeamMember
{
    public string UserId { get; set; }
    public TeamRole Role { get; set; }
    
    public enum TeamRole { Member, Admin }
}
```

### 6. Инструменты для разработчиков игр

```csharp
// GameDevelopmentTools.cs
public class GameDevelopmentTools : MonoBehaviour
{
    // Доступно только для пользователей с ролью Developer
    public bool IsAvailable => UserManager.CurrentUser?.HasDeveloperAccess ?? false;
    
    // Анализ структуры проекта
    public ProjectStructureAnalysis AnalyzeProjectStructure(string projectPath)
    {
        // Анализ структуры проекта Unity
    }
    
    // Генерация документации по коду
    public void GenerateCodeDocumentation(string sourcePath, string outputPath)
    {
        // Генерация документации
    }
    
    // Визуализация зависимостей между компонентами
    public void VisualizeComponentDependencies(Canvas targetCanvas)
    {
        // Создание визуального представления зависимостей
    }
}

// ProjectStructureAnalysis.cs
[Serializable]
public class ProjectStructureAnalysis
{
    public int ScriptCount { get; set; }
    public int SceneCount { get; set; }
    public int PrefabCount { get; set; }
    public Dictionary<string, int> ScriptsByNamespace { get; set; }
    public List<string> UnusedAssets { get; set; }
    public List<string> MissingReferences { get; set; }
}
```

## Рекомендации по работе в команде

### Разделение обязанностей

1. **Опытный разработчик**:
   - Архитектура проекта и ключевые компоненты
   - Реализация сложных систем (сеть, хранение данных)
   - Код-ревью и наставничество
   - Оптимизация производительности

2. **Начинающий разработчик**:
   - UI компоненты и взаимодействие с пользователем
   - Базовая логика приложения
   - Тестирование и отладка
   - Документирование кода

### Методология разработки

Учитывая короткие сроки (2-3 дня), рекомендуется использовать упрощенную Agile методологию:

1. **День 1**: 
   - Утро: Планирование и создание базовой архитектуры (2 часа)
   - День: Реализация основных систем (аутентификация, модели данных) (6 часов)
   - Вечер: Ежедневное ревью и планирование следующего дня (1 час)

2. **День 2**:
   - Утро: Разработка UI компонентов (2 часа)
   - День: Реализация функциональности Kanban и Canvas (6 часов)
   - Вечер: Ежедневное ревью и планирование следующего дня (1 час)

3. **День 3**:
   - Утро: Реализация системы заметок и команд (2 часа)
   - День: Интеграция систем и отладка (4 часа)
   - Вечер: Финальное тестирование и подготовка релиза (3 часа)

### Коммуникация и координация

- Ежедневные встречи (15-30 минут) для синхронизации работы
- Использование системы контроля версий (Git) с четкими правилами ветвления
- Организация совместных сессий программирования для решения сложных задач
- Использование инструментов для трекинга задач (можно использовать само разрабатываемое приложение!)

### Правила работы с Git

```
main       - Стабильная версия приложения
|
|-- develop - Ветка разработки
    |
    |-- feature/auth       - Функциональность аутентификации
    |-- feature/kanban     - Функциональность Kanban-досок
    |-- feature/canvas     - Функциональность Canvas
    |-- feature/notes      - Функциональность заметок
    |-- feature/teams      - Функциональность команд
    |-- feature/dev-tools  - Инструменты для разработчиков
```

- Коммиты должны быть атомарными и иметь осмысленные сообщения
- Перед слиянием ветки необходимо выполнить код-ревью
- Использование Pull Requests для обсуждения изменений

## Интеграция дизайна из Figma в Unity

### Подготовка дизайна в Figma

1. **Организация макетов**:
   - Создайте отдельные страницы для каждого экрана приложения
   - Используйте компоненты для повторяющихся элементов
   - Организуйте слои логически и используйте осмысленные имена

2. **Определение стилей**:
   - Создайте систему дизайна с определением цветов, типографики, размеров и отступов
   - Используйте переменные стилей в Figma для единообразия

3. **Подготовка ассетов для экспорта**:
   - Отметьте элементы для экспорта (иконки, изображения)
   - Используйте векторные форматы где возможно
   - Подготовьте различные размеры (1x, 2x) для растровых изображений

### Экспорт ассетов из Figma

1. **Экспорт изображений**:
   - Выберите элементы для экспорта
   - Установите форматы (PNG для изображений с прозрачностью, JPG для фотографий)
   - Экспортируйте в отдельную папку

2. **Экспорт UI-Kit**:
   - Экспортируйте цветовую палитру (можно через плагин Figma)
   - Экспортируйте информацию о шрифтах и размерах
   - Сохраните значения отступов и размеров компонентов

### Импорт в Unity

1. **Структура папок в Unity**:
   ```
   Assets/
   ├── UI/
   │   ├── Images/        # Изображения интерфейса
   │   ├── Icons/         # Иконки
   │   ├── Fonts/         # Шрифты
   │   └── Themes/        # Темы и стили
   ```

2. **Импорт изображений**:
   - Создайте папку `Assets/UI/Images` и `Assets/UI/Icons`
   - Импортируйте изображения через Drag & Drop или меню Assets > Import New Asset
   - Настройте свойства изображений (Texture Type: Sprite, Compression и т.д.)

3. **Импорт шрифтов**:
   - Создайте папку `Assets/UI/Fonts`
   - Импортируйте TTF или OTF файлы
   - Создайте TMP Font Asset для каждого шрифта (Window > TextMeshPro > Font Asset Creator)

4. **Создание тем**:
   - Создайте скрипт `ThemeManager.cs` для управления темами
   - Определите ScriptableObject для хранения цветов, шрифтов и других параметров дизайна
   - Создайте инспектор для удобного редактирования тем

```csharp
// ThemeData.cs
[CreateAssetMenu(fileName = "Theme", menuName = "Orion/Theme Data")]
public class ThemeData : ScriptableObject
{
    [Header("Colors")]
    public Color primaryColor;
    public Color secondaryColor;
    public Color accentColor;
    public Color backgroundColor;
    public Color surfaceColor;
    public Color textPrimaryColor;
    public Color textSecondaryColor;
    public Color errorColor;
    
    [Header("Typography")]
    public TMP_FontAsset headingFont;
    public TMP_FontAsset bodyFont;
    public float headingSize1;
    public float headingSize2;
    public float headingSize3;
    public float bodySize;
    public float captionSize;
    
    [Header("Spacing")]
    public float spacingSmall;
    public float spacingMedium;
    public float spacingLarge;
    
    [Header("Borders")]
    public float borderRadius;
    public float borderWidth;
}

// ThemeManager.cs
public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }
    
    [SerializeField] private ThemeData _lightTheme;
    [SerializeField] private ThemeData _darkTheme;
    
    private ThemeData _currentTheme;
    
    public ThemeData CurrentTheme => _currentTheme;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // По умолчанию используем светлую тему
        _currentTheme = _lightTheme;
    }
    
    public void SetLightTheme()
    {
        _currentTheme = _lightTheme;
        ApplyTheme();
    }
    
    public void SetDarkTheme()
    {
        _currentTheme = _darkTheme;
        ApplyTheme();
    }
    
    private void ApplyTheme()
    {
        // Обновление всех UI элементов с новой темой
        // Уведомление подписчиков о изменении темы
    }
}

// UIElement.cs - базовый класс для тематических UI элементов
public abstract class ThemedUIElement : MonoBehaviour
{
    protected virtual void Start()
    {
        ApplyTheme(ThemeManager.Instance.CurrentTheme);
    }
    
    protected abstract void ApplyTheme(ThemeData theme);
}

// Пример: ThemedButton.cs
public class ThemedButton : ThemedUIElement
{
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _text;
    
    protected override void ApplyTheme(ThemeData theme)
    {
        _background.color = theme.primaryColor;
        _text.color = theme.textPrimaryColor;
        _text.font = theme.bodyFont;
        _text.fontSize = theme.bodySize;
    }
}
```

### Создание префабов UI элементов

1. Создайте базовые префабы для всех UI элементов в соответствии с дизайном из Figma:
   - Кнопки (разных типов)
   - Поля ввода
   - Карточки
   - Заголовки и текстовые блоки
   - Переключатели и чекбоксы
   - Выпадающие списки

2. Добавьте к каждому префабу соответствующий компонент наследник `ThemedUIElement`

3. Создайте UI Kit сцену с примерами всех UI компонентов для удобного доступа

## Заключение

Проект Orion представляет собой амбициозную задачу, особенно учитывая короткие сроки разработки. Ключом к успеху будет:

1. **Фокусирование на MVP**: Сначала реализуйте минимальный набор функций, затем добавляйте дополнительные возможности
2. **Модульная архитектура**: Разделите системы на независимые модули для параллельной разработки
3. **Регулярная коммуникация**: Обеспечьте постоянный обмен информацией между членами команды
4. **Автоматизация**: Используйте инструменты для автоматизации рутинных задач
5. **Инкрементальное тестирование**: Регулярно тестируйте каждый компонент

При эффективной организации работы и правильном распределении задач, создание базовой версии приложения за 2-3 дня вполне реально, особенно если сосредоточиться на ключевой функциональности и отложить второстепенные задачи на будущие итерации. 