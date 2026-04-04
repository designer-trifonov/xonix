using System;
using UnityEngine;
using UnityEngine.UI;
using HippoGame.Core;

namespace HippoGame.UI
{
    /// Панель выбора сложности перед стартом игры.
    /// Содержит три кнопки: Лёгкая / Средняя / Сложная.
    /// После выбора скрывается и бросает событие OnDifficultySelected.
    public class DifficultySelectUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button     _easyButton;
        [SerializeField] private Button     _mediumButton;
        [SerializeField] private Button     _hardButton;

        public event Action<Difficulty> OnDifficultySelected;

        private void Awake()
        {
            _easyButton  .onClick.AddListener(() => Select(Difficulty.Easy));
            _mediumButton.onClick.AddListener(() => Select(Difficulty.Medium));
            _hardButton  .onClick.AddListener(() => Select(Difficulty.Hard));
        }

        public void Show()  => _panel.SetActive(true);
        public void Hide()  => _panel.SetActive(false);

        private void Select(Difficulty d)
        {
            Hide();
            OnDifficultySelected?.Invoke(d);
        }
    }
}
