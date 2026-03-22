using System;
using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Hippo
{
    /// Отслеживает клетки гиппо на сетке, управляет трейлом,
    /// триггерит заливку зоны, обрабатывает попадания шара.
    /// Трейл строится атомарно по событиям смены направления — без FPS-зависимости.
    public class HippoGridInteractor : MonoBehaviour, IInitializable, IBallInteractable, IHippoGridInteractor, IDrawingState
    {
        private IGridService     _grid;
        private IFillService     _fill;
        private ITrailService    _trail;
        private ITrailRenderer   _trailRenderer;
        private IParticleService _particles;
        private IBallSpawner     _ballSpawner;
        private Transform        _hippoTransform;

        private Vector2Int _lastCell;
        private Vector2Int _segmentStartCell;
        private bool       _isDrawing;
        private bool       _hitInProgress;

        public bool IsVulnerable => _isDrawing && !_hitInProgress;
        public bool IsDrawing    => _isDrawing;

        public event Action OnZoneFilled;
        public event Action OnHit;

        public void Inject(IGridService grid, IFillService fill, ITrailService trail,
            Transform hippoTransform, IMovementBehaviour movement,
            IParticleService particles = null, IBallSpawner ballSpawner = null,
            ITrailRenderer trailRenderer = null)
        {
            _grid           = grid;
            _fill           = fill;
            _trail          = trail;
            _trailRenderer  = trailRenderer;
            _particles      = particles;
            _ballSpawner    = ballSpawner;
            _hippoTransform = hippoTransform;

            movement.OnDirectionChanged += OnMovementDirectionChanged;
            Debug.Log("[HippoGridInteractor] Inject — все зависимости получены");
        }

        public void Initialize()
        {
            _lastCell         = _grid.WorldToCell(_hippoTransform.position);
            _segmentStartCell = _lastCell;
            _isDrawing        = false;
            _hitInProgress    = false;
            Debug.Log($"[HippoGridInteractor] Initialize: startCell=({_lastCell.x},{_lastCell.y})");
        }

        // ── Событие смены направления ───────────────────────────────────────────────
        private void OnMovementDirectionChanged(Vector2 newDir)
        {
            Vector2Int cell = _grid.WorldToCell(_hippoTransform.position);

            if (_isDrawing)
            {
                CommitSegment(_segmentStartCell, cell);
                Debug.Log($"[HippoGridInteractor] Поворот: коммит {_segmentStartCell}→{cell}");
            }

            _segmentStartCell = cell;
        }

        // ── Атомарная запись сегмента в grid ────────────────────────────────────────
        private void CommitSegment(Vector2Int from, Vector2Int to)
        {
            if (from == to) return;

            Vector2Int diff = to - from;
            Vector2Int step;
            int        steps;

            if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
            {
                steps = Mathf.Abs(diff.x);
                step  = new Vector2Int(diff.x > 0 ? 1 : -1, 0);
            }
            else
            {
                steps = Mathf.Abs(diff.y);
                step  = new Vector2Int(0, diff.y > 0 ? 1 : -1);
            }

            for (int i = 0; i <= steps; i++)
            {
                Vector2Int c  = from + step * i;
                if (!_grid.IsInBounds(c)) break;
                CellState  cs = _grid.GetCell(c.x, c.y);
                if (cs == CellState.Empty)
                {
                    _trail.AddPoint(c);
                    _grid.SetCell(c.x, c.y, CellState.Trail);
                }
                else if (cs == CellState.Filled)
                {
                    _trail.AddPoint(c); // визуал — не меняем grid
                }
            }
        }

        // ── Update: детекция старта и закрытия зоны ─────────────────────────────────
        private void Update()
        {
            if (_grid == null || _hippoTransform == null) return;

            Vector2Int cell = _grid.WorldToCell(_hippoTransform.position);
            if (cell == _lastCell) return;

            Vector2Int prevCell = _lastCell;
            _lastCell = cell;

            if (!_grid.IsInBounds(cell)) return;

            HandleCellChange(cell, prevCell);
        }

        private void HandleCellChange(Vector2Int cell, Vector2Int prevCell)
        {
            CellState state  = _grid.GetCell(cell.x, cell.y);
            bool      isEdge = _grid.IsEdge(cell);

            if (_hitInProgress && isEdge)
            {
                _hitInProgress = false;
                Debug.Log($"[HippoGridInteractor] Пристыковался к стене ({cell.x},{cell.y})");
                return;
            }

            if (_isDrawing)
            {
                if (state == CellState.Trail)
                {
                    // Врезались в собственный трейл — смерть
                    ClearTrailCells();
                    _trail.Clear();
                    _trailRenderer?.Clear();
                    _isDrawing     = false;
                    _hitInProgress = true;
                    Debug.Log("[HippoGridInteractor] Врезался в свой трейл — смерть");
                    OnHit?.Invoke();
                    return;
                }

                if (isEdge || state == CellState.Filled)
                {
                    _isDrawing = false;

                    CommitSegment(_segmentStartCell, prevCell);

                    if (isEdge && _grid.GetCell(cell.x, cell.y) == CellState.Empty)
                    {
                        _trail.AddPoint(cell);
                        _grid.SetCell(cell.x, cell.y, CellState.Trail);
                    }

                    Vector3 dockWorld = _grid.CellToWorld(cell);
                    _particles?.PlayDock(new Vector3(dockWorld.x, dockWorld.y, -0.1f));
                    Debug.Log($"[HippoGridInteractor] Трейл ЗАКРЫТ в ({cell.x},{cell.y}) len={_trail.Points.Count}");

                    if (_hitInProgress)
                    {
                        ClearTrailCells();
                        _trail.Clear();
                        _trailRenderer?.Clear();
                        _hitInProgress = false;
                        Debug.Log("[HippoGridInteractor] После хита — без заливки");
                    }
                    else
                    {
                        _fill.Fill(_grid, new List<Vector2Int>(_trail.Points), _ballSpawner?.GetPositions());
                        _trail.Clear();
                        _trailRenderer?.Clear();
                        Debug.Log("[HippoGridInteractor] Заливка выполнена");
                        OnZoneFilled?.Invoke();
                    }
                }
            }
            else
            {
                if (!isEdge && state == CellState.Empty && !_hitInProgress)
                {
                    _isDrawing = true;
                    _trail.Clear();

                    if (_grid.IsEdge(_segmentStartCell) &&
                        _grid.GetCell(_segmentStartCell.x, _segmentStartCell.y) == CellState.Empty)
                    {
                        _trail.AddPoint(_segmentStartCell);
                        _grid.SetCell(_segmentStartCell.x, _segmentStartCell.y, CellState.Trail);
                    }

                    Debug.Log($"[HippoGridInteractor] НАЧАЛО РИСОВАНИЯ segStart={_segmentStartCell}");
                }
            }
        }

        // ── IBallInteractable ────────────────────────────────────────────────────────
        public bool IsNearTrail(Vector2 pos, float threshold)
        {
            var pts = _trail.Points;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                Vector2 a = _grid.CellToWorld(pts[i]);
                Vector2 b = _grid.CellToWorld(pts[i + 1]);
                if (SegmentDist(pos, a, b) < threshold)
                    return true;
            }

            if (_isDrawing && pts.Count > 0)
            {
                Vector2 a = _grid.CellToWorld(_segmentStartCell);
                Vector2 b = _hippoTransform.position;
                if (SegmentDist(pos, a, b) < threshold)
                    return true;
            }

            return false;
        }

        public void OnBallHit(Vector2 hitPosition)
        {
            if (!IsVulnerable) return;

            Debug.Log($"[HippoGridInteractor] OnBallHit @ {hitPosition}");
            _particles?.PlayBallHit(new Vector3(hitPosition.x, hitPosition.y, -0.1f));
            ClearTrailCells();
            _trail.Clear();
            _isDrawing     = false;
            _hitInProgress = true;
            OnHit?.Invoke();
        }

        public void ResetState(Vector3 hippoPosition)
        {
            Debug.Log("[HippoGridInteractor] ResetState");
            ClearTrailCells();
            _trail.Clear();
            _isDrawing        = false;
            _hitInProgress    = false;
            _lastCell         = _grid.WorldToCell(hippoPosition);
            _segmentStartCell = _lastCell;
        }

        private void ClearTrailCells()
        {
            foreach (Vector2Int c in _trail.Points)
                if (_grid.GetCell(c.x, c.y) == CellState.Trail)
                    _grid.SetCell(c.x, c.y, CellState.Empty);
        }

        private static float SegmentDist(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a, ap = p - a;
            float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab.sqrMagnitude);
            return (p - (a + t * ab)).magnitude;
        }
    }
}
