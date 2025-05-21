using UnityEngine;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Компонент для управления видимостью пароля в поле ввода
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class PasswordHide : MonoBehaviour
{
    [Tooltip("Вызывается при изменении состояния видимости пароля. Булевый параметр указывает, виден ли пароль.")]
    public UnityEvent<bool> OnPasswordVisibilityChanged = new UnityEvent<bool>();
    
    private TMP_InputField _inputField;
    private bool _isPasswordVisible = false;
    
    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        
        // Устанавливаем начальный тип поля как пароль
        _inputField.contentType = TMP_InputField.ContentType.Password;
        _inputField.ForceLabelUpdate();
    }
    
    /// <summary>
    /// Переключает видимость пароля и вызывает соответствующее событие
    /// </summary>
    public void TogglePasswordVisibility()
    {
        _isPasswordVisible = !_isPasswordVisible;
        
        // Меняем тип контента в зависимости от состояния
        _inputField.contentType = _isPasswordVisible 
            ? TMP_InputField.ContentType.Standard 
            : TMP_InputField.ContentType.Password;
            
        // Обновляем отображение
        _inputField.ForceLabelUpdate();
        
        // Уведомляем подписчиков об изменении видимости
        OnPasswordVisibilityChanged.Invoke(_isPasswordVisible);
    }
    
    /// <summary>
    /// Установка видимости пароля напрямую
    /// </summary>
    /// <param name="isVisible">Если true, пароль будет отображаться как обычный текст</param>
    public void SetPasswordVisibility(bool isVisible)
    {
        if (_isPasswordVisible == isVisible) return;
        
        _isPasswordVisible = isVisible;
        
        // Меняем тип контента в зависимости от состояния
        _inputField.contentType = _isPasswordVisible 
            ? TMP_InputField.ContentType.Standard 
            : TMP_InputField.ContentType.Password;
            
        // Обновляем отображение
        _inputField.ForceLabelUpdate();
        
        // Уведомляем подписчиков об изменении видимости
        OnPasswordVisibilityChanged.Invoke(_isPasswordVisible);
    }
    
    /// <summary>
    /// Показать пароль
    /// </summary>
    public void ShowPassword()
    {
        SetPasswordVisibility(true);
    }
    
    /// <summary>
    /// Скрыть пароль
    /// </summary>
    public void HidePassword()
    {
        SetPasswordVisibility(false);
    }
    
    /// <summary>
    /// Проверка, виден ли сейчас пароль
    /// </summary>
    public bool IsPasswordVisible => _isPasswordVisible;
} 