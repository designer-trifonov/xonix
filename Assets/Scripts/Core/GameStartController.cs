using System;
using System.Collections.Generic;
using UnityEngine;
using YG;
using HippoGame.UI;
using HippoGame.Interfaces;

namespace HippoGame.Core
{
    /// Управляет стартовой последовательностью:
    /// RulesPopup → DifficultySelect → запуск игры.
    public class GameStartController : MonoBehaviour
    {
        [SerializeField] private RulesPopupController         _rulesPopup;
        [SerializeField] private DifficultySelectUIController _difficultySelect;
        [SerializeField] private GameObject[]                 _hideUntilStart;
        [SerializeField] private UnityEngine.UI.Button        _mainMenuButton;

        private readonly List<Action> _initCallbacks = new();

        private IGameState         _gameState;
        private ISaveManager       _saveManager;
        private ISnapshotCollector _snapshotCollector;
        private IGameRestorer      _restorer;

        public void Inject(IGameState gameState,
            ISaveManager saveManager = null,
            ISnapshotCollector snapshotCollector = null,
            IGameRestorer restorer = null)
        {
            _gameState         = gameState;
            _saveManager       = saveManager;
            _snapshotCollector = snapshotCollector;
            _restorer          = restorer;
        }

        public void SetMainMenuAction(Action action)
        {
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(() => action?.Invoke());
        }

        public void RegisterInit(Action callback) => _initCallbacks.Add(callback);

        private void Start() => SetVisible(false);

        // ── Точка входа ───────────────────────────────────────────────────────────────

        public void BeginFlow()
        {
            // YG2 SDK грузит сейвы асинхронно — если ещё не готов, ждём
            if (!YG2.isSDKEnabled)
            {
                Debug.Log("[GameStartController] YG2 SDK не готов — ждём onGetSDKData");
                YG2.onGetSDKData += BeginFlow;
                return;
            }
            YG2.onGetSDKData -= BeginFlow; // отписываемся на случай повторного вызова

            if (_saveManager != null && _saveManager.HasSavedGame)
            {
                var snap = _saveManager.LoadSnapshot();

                // Восстанавливаем GameState ДО инициализации систем
                _gameState.RestoreSession(snap.Score, snap.Level, snap.Lives, (Difficulty)snap.Difficulty);

                SetVisible(true);
                RunInitCallbacks();

                // Восстанавливаем позиции/поле ПОСЛЕ инициализации
                _restorer?.Restore(snap);

                _snapshotCollector?.Activate();
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
            SetVisible(false);

            if (_difficultySelect != null)
            {
                void Handler(Difficulty d)
                {
                    _difficultySelect.OnDifficultySelected -= Handler;
                    _gameState?.SetDifficulty(d);
                    SetVisible(true);
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

        // ── Приватный поток ───────────────────────────────────────────────────────────

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

        private void OnDifficultyChosen(Difficulty d)
        {
            _difficultySelect.OnDifficultySelected -= OnDifficultyChosen;
            _gameState?.SetDifficulty(d);
            SetVisible(true);
            RunInitCallbacks();
            _snapshotCollector?.Activate();
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
