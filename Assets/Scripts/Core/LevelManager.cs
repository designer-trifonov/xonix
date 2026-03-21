using System;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Core
{
    /// Логика смены уровней, сброс поля, проверка победы и поражения.
    public class LevelManager
    {
        private IGameState           _gameState;
        private IGridService         _grid;
        private IBallSpawner         _ballSpawner;
        private IHippoController     _hippo;
        private IHippoGridInteractor _interactor;
        private IMovementBehaviour   _movement;
        private IBoundaryService     _boundary;

        private bool _levelTransitionPending;

        public event Action OnGameOver;

        public void Inject(IGameState state, IGridService grid,
            IBallSpawner spawner, IHippoController hippo,
            IHippoGridInteractor interactor, IMovementBehaviour movement,
            IBoundaryService boundary)
        {
            _gameState   = state;
            _grid        = grid;
            _ballSpawner = spawner;
            _hippo       = hippo;
            _interactor  = interactor;
            _movement    = movement;
            _boundary    = boundary;
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

            Debug.Log($"[LevelManager] Уровень {_gameState.Level} старт | скорость гиппо={_gameState.HippoSpeed:F1} шары={_gameState.BallCount} цель={_gameState.RequiredFillPercent:F0}%");
        }

        private void OnZoneFilled()
        {
            _movement.Stop();
            _gameState.ZoneFilled();

            if (_ballSpawner != null && _ballSpawner.CheckBallsAfterFill())
            {
                Debug.Log("[LevelManager] Все шары закрашены — победа!");
                CompleteLevel();
                return;
            }

            float pct = CountFillPercent();
            Debug.Log($"[LevelManager] Зона залита | {pct:F1}% / {_gameState.RequiredFillPercent:F0}%");

            if (pct >= _gameState.RequiredFillPercent)
                CompleteLevel();
        }

        private void CompleteLevel()
        {
            if (_levelTransitionPending) return;
            _levelTransitionPending = true;

            Debug.Log($"[LevelManager] Уровень {_gameState.Level} завершён!");
            _gameState.NextLevel();
            ResetField();
            _levelTransitionPending = false;
            StartLevel();
        }

        private void OnGameStateChanged()
        {
            if (_gameState.Lives <= 0)
            {
                Debug.Log("[LevelManager] Жизни кончились — GameOver");
                OnGameOver?.Invoke();
            }
        }

        public void RestartFromLevel1()
        {
            Debug.Log("[LevelManager] Рестарт с уровня 1");
            _gameState.Reset();
            ResetField();
            StartLevel();
        }

        public void ContinueAfterAd()
        {
            Debug.Log("[LevelManager] Продолжение после рекламы — восстановление жизней");
            _gameState.RestoreLives();
            StartLevel();
        }

        private void ResetField()
        {
            Debug.Log("[LevelManager] ResetField — сетка, трейл, шары");
            _grid.ResetCells();

            Vector3 spawnPos = new Vector3(0f, _boundary.GetBounds().yMax, 0f);
            _hippo.SetPosition(spawnPos);
            _interactor.ResetState(spawnPos);

            _ballSpawner?.ClearBalls();
        }

        private float CountFillPercent()
        {
            int total = _grid.Columns * _grid.Rows;
            if (total == 0) return 0f;

            int filled = 0;
            for (int x = 0; x < _grid.Columns; x++)
            for (int y = 0; y < _grid.Rows; y++)
                if (_grid.GetCell(x, y) == CellState.Filled)
                    filled++;

            return (float)filled / total * 100f;
        }
    }
}
