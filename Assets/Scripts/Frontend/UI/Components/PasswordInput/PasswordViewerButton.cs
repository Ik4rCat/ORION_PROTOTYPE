using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Orion.Frontend.UI.Components
{
    /// <summary>
    /// Управляет кнопкой для переключения видимости пароля в поле ввода
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PasswordViewerButton : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _passwordField;
        
        [Tooltip("Спрайт для скрытого пароля (глаз перечеркнут)")]
        [SerializeField] private Sprite _hiddenPasswordSprite;
        
        [Tooltip("Спрайт для видимого пароля (глаз открыт)")]
        [SerializeField] private Sprite _visiblePasswordSprite;
        
        private Button _button;
        private Image _buttonImage;
        private bool _isPasswordVisible = false;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
            
            // Проверяем, что все необходимые компоненты присутствуют
            if (_passwordField == null)
            {
                Debug.LogError("PasswordViewerButton: Не указано поле ввода пароля!");
                return;
            }
            
            if (_hiddenPasswordSprite == null || _visiblePasswordSprite == null)
            {
                Debug.LogWarning("PasswordViewerButton: Не указаны спрайты для кнопки!");
            }
            
            // Устанавливаем стартовый спрайт (пароль скрыт)
            if (_buttonImage != null && _hiddenPasswordSprite != null)
            {
                _buttonImage.sprite = _hiddenPasswordSprite;
            }
            
            // Добавляем обработчик нажатия на кнопку
            if (_button != null)
            {
                _button.onClick.AddListener(TogglePasswordVisibility);
            }
            
            // Убеждаемся, что поле ввода настроено как пароль по умолчанию
            if (_passwordField != null)
            {
                _passwordField.contentType = TMP_InputField.ContentType.Password;
                _passwordField.ForceLabelUpdate(); // Обновляем отображение
            }
        }
        
  
        /// Переключает видимость пароля и меняет спрайт кнопки
        public void TogglePasswordVisibility()
        {
            if (_passwordField == null) return;
            
            _isPasswordVisible = !_isPasswordVisible;
            
            // Меняем тип контента поля ввода
            _passwordField.contentType = _isPasswordVisible 
                ? TMP_InputField.ContentType.Standard 
                : TMP_InputField.ContentType.Password;
                
            // Обновляем отображение поля ввода
            _passwordField.ForceLabelUpdate();
            
            // Меняем спрайт кнопки
            if (_buttonImage != null)
            {
                _buttonImage.sprite = _isPasswordVisible 
                    ? _visiblePasswordSprite 
                    : _hiddenPasswordSprite;
            }
        }
        
        private void OnDestroy()
        {
            // Очищаем обработчик нажатия
            if (_button != null)
            {
                _button.onClick.RemoveListener(TogglePasswordVisibility);
            }
        }
    }
}
