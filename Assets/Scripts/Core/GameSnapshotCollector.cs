using System;
using System.Collections.Generic;
using UnityEngine;
using YG;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Core
{
    /// Единственный сборщик данных для сохранения.
    /// Знает обо всех источниках данных, собирает снимок и шлёт событие.
    /// Не знает как сохранять и как восстанавливать.
    public class GameSnapshotCollector : MonoBehaviour, ISnapshotCollector
    {
        public event Action<GameSnapshot> OnSnapshot;

        private IGameState           _gameState;
        private IGridService         _grid;
        private ILevelManager        _levelManager;
        private IHippoController     _hippo;
        private IMovementBehaviour   _movement;
        private IHippoGridInteractor _interactor;
        private ITrailService        _trail;
        private IBallSpawner         _ballSpawner;
        private bool                 _active;

        public void Inject(
            IGameState gameState, IGridService grid, ILevelManager levelManager,
            IHippoController hippo, IMovementBehaviour movement,
            IHippoGridInteractor interactor, ITrailService trail, IBallSpawner ballSpawner)
        {
            _gameState   = gameState;
            _grid        = grid;
            _levelManager = levelManager;
            _hippo       = hippo;
            _movement    = movement;
            _interactor  = interactor;
            _trail       = trail;
            _ballSpawner = ballSpawner;
        }

        // ── YG2-события подписываются всегда, сбор идёт только при _active ──────────

        private void OnEnable()
        {
            YG2.onPauseGame       += OnPause;
            YG2.onFocusWindowGame += OnFocus;
        }

        private void OnDisable()
        {
            YG2.onPauseGame       -= OnPause;
            YG2.onFocusWindowGame -= OnFocus;
        }

        // ── Активация/деактивация — управляет подпиской на игровые события ──────────

        public void Activate()
        {
            if (_active) return;
            _active = true;
            _gameState.OnChanged         += Capture;
            _levelManager.OnLevelStarted += Capture;
            Debug.Log("[SnapshotCollector] Активирован");
        }

        public void Deactivate()
        {
            if (!_active) return;
            _active = false;
            _gameState.OnChanged         -= Capture;
            _levelManager.OnLevelStarted -= Capture;
            Debug.Log("[SnapshotCollector] Деактивирован");
        }

        // ── Сбор ─────────────────────────────────────────────────────────────────────

        public void Capture()
        {
            if (!_active) return;
            var snap = Build();
            OnSnapshot?.Invoke(snap);
        }

        private GameSnapshot Build()
        {
            var snap = new GameSnapshot
            {
                Score       = _gameState.Score,
                Level       = _gameState.Level,
                Lives       = _gameState.Lives,
                Difficulty  = (int)_gameState.Difficulty,
                LastFillPct = _levelManager.LastFillPct,
                GridCells   = SerializeGrid(),
                HippoCell   = _grid.WorldToCell(_hippo.Position),
                HippoDir    = _movement.Direction,
                IsDrawing   = _interactor.IsDrawing,
                TrailPoints = new List<Vector2Int>(_trail.Points),
                BallSpeed   = _ballSpawner.GetCurrentSpeed(),
            };

            foreach (var (pos, dir) in _ballSpawner.GetBallsData())
                snap.Balls.Add(new BallSnapshot(pos, dir));

            return snap;
        }

        // ── Сериализация поля ─────────────────────────────────────────────────────────

        private string SerializeGrid()
        {
            int    cols = _grid.Columns;
            int    rows = _grid.Rows;
            byte[] data = new byte[cols * rows];
            for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
                data[x * rows + y] = (byte)_grid.GetCell(x, y);
            return System.Convert.ToBase64String(data);
        }

        private void OnPause(bool isPause)   { if (isPause)    Capture(); }
        private void OnFocus(bool hasFocus)  { if (!hasFocus)  Capture(); }
    }
}
