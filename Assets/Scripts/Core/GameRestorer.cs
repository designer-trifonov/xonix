using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Core
{
    /// Единственное место восстановления состояния игры из снимка.
    /// Получает GameSnapshot — раздаёт данные каждой подсистеме.
    /// Не знает как сохранять и откуда брать данные.
    public class GameRestorer : IGameRestorer
    {
        private IGridService         _grid;
        private IHippoController     _hippo;
        private IMovementBehaviour   _movement;
        private IHippoGridInteractor _interactor;
        private ILevelManager        _levelManager;
        private IBallSpawner         _ballSpawner;
        private ITrailRenderer       _trailRenderer;

        public void Inject(
            IGridService grid, IHippoController hippo, IMovementBehaviour movement,
            IHippoGridInteractor interactor, ILevelManager levelManager,
            IBallSpawner ballSpawner, ITrailRenderer trailRenderer = null)
        {
            _grid          = grid;
            _hippo         = hippo;
            _movement      = movement;
            _interactor    = interactor;
            _levelManager  = levelManager;
            _ballSpawner   = ballSpawner;
            _trailRenderer = trailRenderer;
        }

        public void Restore(GameSnapshot snap)
        {
            RestoreGrid(snap);
            RestoreHippo(snap);
            RestoreTrail(snap);
            RestoreBalls(snap);
            Debug.Log($"[GameRestorer] Restored: level={snap.Level} hippo={snap.HippoCell} balls={snap.Balls.Count} drawing={snap.IsDrawing}");
        }

        // ── Поле ─────────────────────────────────────────────────────────────────────

        private void RestoreGrid(GameSnapshot snap)
        {
            _levelManager.RestoreLastFillPct(snap.LastFillPct);

            if (string.IsNullOrEmpty(snap.GridCells)) return;
            byte[] data = System.Convert.FromBase64String(snap.GridCells);
            int    cols = _grid.Columns;
            int    rows = _grid.Rows;

            for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
            {
                int idx = x * rows + y;
                if (idx < data.Length)
                    _grid.SetCell(x, y, (CellState)data[idx]);
            }
            _grid.RefreshRenderer();
        }

        // ── Гиппо ────────────────────────────────────────────────────────────────────

        private void RestoreHippo(GameSnapshot snap)
        {
            if (snap.HippoCell == Vector2Int.zero && snap.HippoDir == Vector2Int.zero) return;

            Vector2 w   = _grid.CellToWorld(snap.HippoCell);
            Vector3 pos = new Vector3(w.x, w.y, -1f);

            _hippo.SetPosition(pos);
            _movement.RestoreState(snap.HippoCell, snap.HippoDir);
        }

        // ── Трейл ────────────────────────────────────────────────────────────────────

        private void RestoreTrail(GameSnapshot snap)
        {
            _interactor.RestoreDrawingState(snap.HippoCell, snap.IsDrawing, snap.TrailPoints);

            if (_trailRenderer == null || !snap.IsDrawing || snap.TrailPoints.Count == 0)
                return;

            var worldPts = new List<Vector3>(snap.TrailPoints.Count);
            foreach (var pt in snap.TrailPoints)
            {
                Vector2 w = _grid.CellToWorld(pt);
                worldPts.Add(new Vector3(w.x, w.y, -0.1f));
            }
            _trailRenderer.RestorePositions(worldPts);
        }

        // ── Арбузы ───────────────────────────────────────────────────────────────────

        private void RestoreBalls(GameSnapshot snap)
        {
            if (snap.Balls.Count == 0 || snap.BallSpeed <= 0f) return;

            var data = new List<(Vector2 pos, Vector2 dir)>(snap.Balls.Count);
            foreach (var b in snap.Balls)
                data.Add((b.Position, b.Direction));

            _ballSpawner.SpawnBallsAtData(data, snap.BallSpeed);
        }
    }
}
