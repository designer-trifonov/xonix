using System;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;
using static HippoGame.Grid.GridUtils;

namespace HippoGame.Core
{
    /// Логика смены уровней, сброс поля, проверка победы и поражения.
    public class LevelManager : ILevelManager
    {
        private IGameState           _gameState;
        private IGridService         _grid;
        private IBallSpawner         _ballSpawner;
        private IHippoController     _hippo;
        private IHippoGridInteractor _interactor;
        private IMovementBehaviour   _movement;
        private IBoundaryService     _boundary;
        private IAdService           _adService;

        private bool   _levelTransitionPending;
        private float  _lastFillPct;
        private Action _respawnAction;

        public float LastFillPct => _lastFillPct;

        public event Action OnGameOver;
        public event Action OnLevelComplete;
        public event Action OnLevelStarted;

        public void Inject(IGameState state, IGridService grid,
            IBallSpawner spawner, IHippoController hippo,
            IHippoGridInteractor interactor, IMovementBehaviour movement,
            IBoundaryService boundary, IAdService adService = null)
        {
            _gameState   = state;
            _grid        = grid;
            _ballSpawner = spawner;
            _hippo       = hippo;
            _interactor  = interactor;
            _movement    = movement;
            _boundary    = boundary;
            _adService   = adService;
            Debug.Log("[LevelManager] Inject — все зависимости получены");
        }

        public void Initialize()
        {
            _gameState.OnChanged     += OnGameStateChanged;
            _interactor.OnZoneFilled += OnZoneFilled;
            StartLevel();
            Debug.Log("[LevelManager] Initialize — подписки установлены, уровень запущен");
        }

        private void StartLevel()
        {
            _movement.Speed = _gameState.HippoSpeed;

            if (_ballSpawner != null)
                _ballSpawner.SpawnBalls(_gameState.BallCount, _gameState.BallSpeed);
            else
                Debug.LogWarning("[LevelManager] IBallSpawner == null — шары не созданы");

            OnLevelStarted?.Invoke();
            Debug.Log($"[LevelManager] Уровень {_gameState.Level} старт | скорость гиппо={_gameState.HippoSpeed:F1} шары={_gameState.BallCount} цель={_gameState.RequiredFillPercent:F0}%");
        }

        private void OnZoneFilled()
        {
            _movement.Stop();

            float pct   = CountFillPercent(_grid);
            float delta = pct - _lastFillPct;
            _lastFillPct = pct;

            _gameState.ZoneFilled(delta);

            Debug.Log($"[LevelManager] Зона залита | {pct:F1}% (+{delta:F1}%) / {_gameState.RequiredFillPercent:F0}%");

            if (pct >= _gameState.RequiredFillPercent)
                CompleteLevel();
        }

        private void CompleteLevel()
        {
            if (_levelTransitionPending) return;
            _levelTransitionPending = true;

            Debug.Log($"[LevelManager] Уровень {_gameState.Level} завершён!");
            OnLevelComplete?.Invoke();
            _gameState.NextLevel();
            ResetField();
            StartLevel();
            _levelTransitionPending = false;
        }

        private void OnGameStateChanged()
        {
            if (_gameState.Lives <= 0)
            {
                Debug.Log("[LevelManager] Жизни кончились — GameOver");
                HideGameField();
                OnGameOver?.Invoke();
            }
        }

        private void HideGameField()
        {
            _ballSpawner?.SetBallsVisible(false);
            _hippo?.SetVisible(false);
            _interactor?.ResetState(Vector3.zero);   // очищает трейл-линию
        }

        /// Вызывается перед экраном выбора сложности при рестарте:
        /// очищает визуал поля, не запуская уровень.
        public void ClearFieldVisuals()
        {
            _lastFillPct = 0f;
            _grid.ResetCells();
            _ballSpawner?.ClearBalls();
            _interactor?.ResetState(Vector3.zero);
        }

        public void RestartFromLevel1()
        {
            Debug.Log("[LevelManager] Рестарт с уровня 1");
            _hippo?.SetVisible(true);
            _gameState.Reset();
            ResetField();
            StartLevel();
        }

        public void RestoreLastFillPct(float pct) => _lastFillPct = pct;

        public void SetRespawnAction(Action respawn) => _respawnAction = respawn;

        public void ContinueAfterAd()
        {
            Debug.Log("[LevelManager] ContinueAfterAd — показываем rewarded");
            _adService?.ShowRewarded("continue", () =>
            {
                Debug.Log("[LevelManager] Rewarded выдана — восстановление жизней + телепорт");
                _hippo?.SetVisible(true);
                _ballSpawner?.SetBallsVisible(true);
                _gameState.RestoreLives();
                _respawnAction?.Invoke();
            });
        }

        private void ResetField()
        {
            Debug.Log("[LevelManager] ResetField — сетка, трейл, шары");
            _lastFillPct = 0f;
            _grid.ResetCells();

            Vector3    raw      = new Vector3(0f, _boundary.GetBounds().yMax, 0f);
            Vector2Int cell     = _grid.WorldToCell(raw);
            Vector2    snapped  = _grid.CellToWorld(cell);
            Vector3    spawnPos = new Vector3(snapped.x, snapped.y, -1f);

            _hippo.SetPosition(spawnPos);
            _hippo.ResetMovement();
            _interactor.ResetState(spawnPos);

            _ballSpawner?.ClearBalls();
        }
    }
}
