using System;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Core;

namespace HippoGame.Movement
{
    /// Клеточное движение с плавной визуальной интерполяцией.
    /// Логика дискретна (по клеткам), визуально — плавное скольжение.
    public class CellMovement : IMovementBehaviour
    {
        private readonly IGridService _grid;

        // Логическая позиция (текущая клетка)
        private Vector2Int _cell;
        private Vector2Int _prevCell;
        private Vector2Int _direction;
        private Vector2Int _pending;
        private bool       _initialized;

        // Визуальная интерполяция
        private Vector3 _visualFrom;
        private Vector3 _visualTo;
        private float   _visualT;       // 0..1
        private bool    _interpolating;

        public float      Speed     { get; set; } = 5f;
        public bool       Stopped   { get; private set; }
        public Vector2Int Direction => _direction;

        public event Action<Vector2Int>             OnDirectionChanged;
        public event Action<Vector2Int, Vector2Int> OnCellChanged;

        public CellMovement(IGridService grid)
        {
            _grid = grid;
            _cell = new Vector2Int(grid.Columns / 2, grid.Rows - 1);
        }

        public void QueueDirection(Vector2Int dir) => _pending = dir;

        public void Stop()
        {
            Stopped      = true;
            _pending     = Vector2Int.zero;
            _interpolating = false;
        }

        public void Resume(Transform target = null)
        {
            Stopped        = false;
            _pending       = Vector2Int.zero;
            _direction     = Vector2Int.zero;
            _interpolating = false;

            if (target != null)
            {
                _cell        = _grid.WorldToCell(target.position);
                _visualFrom  = target.position;
                _visualTo    = target.position;
                _initialized = true;
            }
            else
            {
                _initialized = false;
            }
        }

        private bool ApplyPending()
        {
            if (_pending == Vector2Int.zero || _pending == _direction)
            {
                _pending = Vector2Int.zero;
                return false;
            }

            _direction = _pending;
            _pending   = Vector2Int.zero;
            Stopped    = false;
            OnDirectionChanged?.Invoke(_direction);
            return true;
        }

        public void Tick(Transform target, Rect bounds)
        {
            if (!_initialized)
            {
                _cell        = _grid.WorldToCell(target.position);
                _visualFrom  = target.position;
                _visualTo    = target.position;
                _initialized = true;
            }

            // Старт или стоп — применяем pending
            if (_direction == Vector2Int.zero || Stopped)
            {
                if (_pending != Vector2Int.zero)
                {
                    if (_pending == _direction) { Stopped = false; _pending = Vector2Int.zero; }
                    else ApplyPending();
                }
                if (_direction == Vector2Int.zero || Stopped) return;
            }

            // Плавная интерполяция к следующей клетке
            float stepTime = _grid.CellSize / Speed;

            if (!_interpolating)
            {
                // Применяем буфер, берём следующую клетку
                ApplyPending();

                Vector2Int next = _cell + _direction;

                if (!_grid.IsInBounds(next))
                {
                    Stopped = true;
                    return;
                }

                Vector2 nextWorld = _grid.CellToWorld(next);

                _prevCell      = _cell;
                _cell          = next;
                _visualFrom    = target.position;
                _visualTo      = new Vector3(nextWorld.x, nextWorld.y, target.position.z);
                _visualT       = 0f;
                _interpolating = true;
            }

            // Двигаем визуально
            _visualT += Time.deltaTime / stepTime;

            if (_visualT >= 1f)
            {
                _visualT       = 1f;
                _interpolating = false;
                OnCellChanged?.Invoke(_prevCell, _cell);
            }

            target.position = Vector3.Lerp(_visualFrom, _visualTo, _visualT);
        }

        // IMovementBehaviour — совместимость
        public void Tick(Transform t, ref Vector2Int dir, Vector2Int input, Rect b, ICollisionService _)
        {
            if (input != Vector2Int.zero) _pending = input;
            Tick(t, b);
            dir = _direction;
        }
    }
}
