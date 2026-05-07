using System;
using System.Collections.Generic;
using UnityEngine;
using HippoGame.UI;
using HippoGame.Interfaces;

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
        private IGameState      _gameState;
        private GameSessionSaver _saver;

        public void Inject(IGameState gameState, GameSessionSaver saver = null)
        {
            _gameState = gameState;
            _saver     = saver;
        }

        public void RegisterInit(Action callback) => _initCallbacks.Add(callback);

        private void Start()
        {
            SetVisible(false);
        }

        public void BeginFlow()
        {
            if (_saver != null && _saver.HasSavedGame)
            {
                _saver.RestoreToGameState();
                SetVisible(true);
                RunInitCallbacks();
                _saver.RestoreGridAndFill();
                _saver.Activate();
                Debug.Log("[GameStartController] Сессия восстановлена из сохранения");
                return;
            }

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

        public void ShowDifficultyForRestart(Action onReady)
        {
            SetVisible(false);   // прячем HUD — как будто только зашли

            if (_difficultySelect != null)
            {
                void Handler(Difficulty d)
                {
                    _difficultySelect.OnDifficultySelected -= Handler; // отписываемся сразу
                    _gameState?.SetDifficulty(d);
                    SetVisible(true);  // возвращаем HUD перед стартом
                    onReady?.Invoke();
                }
                _difficultySelect.OnDifficultySelected += Handler;
                _difficultySelect.Show();
            }
            else
            {
                SetVisible(true);
                onReady?.Invoke();
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
                OnDifficultyChosen(Difficulty.Medium);
            }
        }

        // Сложность выбрана — применяем и запускаем игру (один раз!)
        private void OnDifficultyChosen(Difficulty d)
        {
            _difficultySelect.OnDifficultySelected -= OnDifficultyChosen; // отписываемся сразу
            _gameState?.SetDifficulty(d);
            SetVisible(true);
            RunInitCallbacks();
            _saver?.Activate();
            Debug.Log($"[GameStartController] Игра запущена | сложность={d}");
        }

        private void RunInitCallbacks()
        {
            foreach (var cb in _initCallbacks)
                cb?.Invoke();
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
