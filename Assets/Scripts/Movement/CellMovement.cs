using System;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Core;

namespace HippoGame.Movement
{
    /// Чистое клеточное движение. Хранит направление, двигает по клеткам.
    /// Никаких проверок — только движение.
    public class CellMovement : IMovementBehaviour
    {
        private readonly IGridService _grid;
        private Vector2Int _cell;
        private Vector2Int _direction;
        private Vector2Int _pending;
        private bool       _initialized;
        private float      _timer;

        public float      Speed     { get; set; } = 5f;
        public bool       Stopped   { get; private set; }
        public Vector2Int Direction => _direction;

        public event Action<Vector2Int>             OnDirectionChanged;
        public event Action<Vector2Int, Vector2Int> OnCellChanged;

        public CellMovement(IGridService grid) => _grid = grid;

        public void QueueDirection(Vector2Int dir) => _pending = dir;

        public void Stop()
        {
            Stopped  = true;
            _pending = Vector2Int.zero;
            _timer   = 0f;
        }

        public void Resume()
        {
            Stopped      = false;
            _initialized = false;
            _pending     = Vector2Int.zero;
        }

        /// Применяет pending если есть. Возвращает true если направление сменилось.
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
                _initialized = true;
            }

            // Старт из нуля или из стопа — применяем сразу
            if (_direction == Vector2Int.zero || Stopped)
            {
                if (_pending != Vector2Int.zero)
                {
                    if (_pending == _direction) { Stopped = false; _pending = Vector2Int.zero; }
                    else ApplyPending();
                }
                if (_direction == Vector2Int.zero || Stopped) return;
            }

            _timer += Time.deltaTime;
            float stepTime = _grid.CellSize / Speed;

            while (_timer >= stepTime)
            {
                _timer -= stepTime;

                // На границе клетки — применяем буфер
                ApplyPending();

                // Шаг
                Vector2Int next      = _cell + _direction;
                Vector2    nextWorld = _grid.CellToWorld(next);

                if (nextWorld.x < bounds.xMin || nextWorld.x > bounds.xMax ||
                    nextWorld.y < bounds.yMin || nextWorld.y > bounds.yMax)
                {
                    Stopped = true;
                    _timer  = 0f;
                    break;
                }

                var prevCell = _cell;
                _cell           = next;
                target.position = new Vector3(nextWorld.x, nextWorld.y, target.position.z);
                OnCellChanged?.Invoke(prevCell, _cell);
            }
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
