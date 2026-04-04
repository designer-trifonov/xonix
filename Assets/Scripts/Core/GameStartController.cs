using System;
using System.Collections.Generic;
using UnityEngine;
using HippoGame.UI;
using HippoGame.Interfaces;
using HippoGame.Core;

namespace HippoGame.Core
{
    /// Управляет стартовой последовательностью:
    /// RulesPopup → DifficultySelect → запуск игры.
    public class GameStartController : MonoBehaviour
    {
        [SerializeField] private RulesPopupController        _rulesPopup;
        [SerializeField] private DifficultySelectUIController _difficultySelect;
        [SerializeField] private GameObject[]                _hideUntilStart;

        private readonly List<Action> _initCallbacks = new();
        private IAdService  _adService;
        private IGameState  _gameState;

        public void Inject(IAdService adService, IGameState gameState)
        {
            _adService  = adService;
            _gameState  = gameState;
        }

        public void RegisterInit(Action callback) => _initCallbacks.Add(callback);

        private void Start()
        {
            SetVisible(false);

            if (_rulesPopup != null)
            {
                _rulesPopup.OnClose += OnRulesClosed;
                _rulesPopup.Show();
            }
            else
            {
                OnRulesClosed();
            }
        }

        // Правила закрыты — показываем выбор сложности
        private void OnRulesClosed()
        {
            if (_difficultySelect != null)
            {
                _difficultySelect.OnDifficultySelected += OnDifficultyChosen;
                _difficultySelect.Show();
            }
            else
            {
                // Нет панели сложности — стартуем со средней
                OnDifficultyChosen(Difficulty.Medium);
            }
        }

        // Сложность выбрана — применяем и запускаем игру
        private void OnDifficultyChosen(Difficulty d)
        {
            _gameState?.SetDifficulty(d);
            _adService?.ShowInterstitial();
            SetVisible(true);
            foreach (var cb in _initCallbacks)
                cb?.Invoke();
            Debug.Log($"[GameStartController] Игра запущена | сложность={d}");
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }

        private void SetVisible(bool visible)
        {
            if (_hideUntilStart == null) return;
            foreach (var go in _hideUntilStart)
                if (go != null) go.SetActive(visible);
        }
    }
}
