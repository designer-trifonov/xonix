using System;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Movement
{
    /// Движение строго по клеткам: позиция всегда CellToWorld(int, int) — без float дрейфа.
    public class CellMovement : IMovementBehaviour
    {
        private readonly IGridService _grid;
        private Vector2Int _cell;
        private bool       _initialized;
        private float      _timer;

        public float Speed   { get; set; } = 5f;
        public bool  Stopped { get; private set; }

        public event Action<Vector2> OnDirectionChanged;

        public CellMovement(IGridService grid) => _grid = grid;

        public void Stop()   { Stopped = true; }
        public void Resume() { Stopped = false; }

        public void Tick(Transform target, ref Vector2 currentDir, Vector2 inputDir,
            Rect bounds, ICollisionService _)
        {
            // Ленивая инициализация: один раз читаем текущую клетку из позиции
            if (!_initialized)
            {
                _cell        = _grid.WorldToCell(target.position);
                _initialized = true;
            }

            // Смена направления
            if (inputDir != Vector2.zero)
            {
                if (inputDir != currentDir)
                {
                    currentDir = inputDir;
                    Stopped    = false;
                    OnDirectionChanged?.Invoke(currentDir);
                }
                else if (Stopped)
                {
                    Stopped = false;
                }
            }

            if (currentDir == Vector2.zero || Stopped) return;

            _timer += Time.deltaTime;
            float stepTime = _grid.CellSize / Speed;

            while (_timer >= stepTime)
            {
                _timer -= stepTime;

                var step = new Vector2Int(
                    Math.Sign(currentDir.x),
                    Math.Sign(currentDir.y));

                Vector2Int next      = _cell + step;
                Vector2    nextWorld = _grid.CellToWorld(next);

                if (nextWorld.x < bounds.xMin || nextWorld.x > bounds.xMax ||
                    nextWorld.y < bounds.yMin || nextWorld.y > bounds.yMax)
                {
                    Stopped = true;
                    break;
                }

                _cell            = next;
                target.position  = new Vector3(nextWorld.x, nextWorld.y, target.position.z);
            }
        }
    }
}
