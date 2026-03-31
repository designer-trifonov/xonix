using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using HippoGame.Interfaces;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace HippoGame.Input
{
    /// Свайп-управление для мобильных.
    /// Направление фиксируется при отпускании пальца.
    /// Диагональ: если X и Y компоненты оба > DiagonalThreshold.
    public class TouchInputProvider : IInputProvider
    {
        // Минимальная длина свайпа в пикселях
        private const float MinSwipeDistance = 30f;

        // Если нормализованный компонент >= этого порога — ось активна (диагональ)
        private const float DiagonalThreshold = 0.35f;

        private Vector2Int _pendingDirection;

        public TouchInputProvider()
        {
            EnhancedTouchSupport.Enable();
        }

        public Vector2Int GetDirection()
        {
            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    Vector2 delta = touch.screenPosition - touch.startScreenPosition;

                    if (delta.magnitude < MinSwipeDistance)
                        continue;

                    Vector2 norm = delta.normalized;

                    int x = Mathf.Abs(norm.x) >= DiagonalThreshold ? (int)Mathf.Sign(norm.x) : 0;
                    int y = Mathf.Abs(norm.y) >= DiagonalThreshold ? (int)Mathf.Sign(norm.y) : 0;

                    if (x == 0 && y == 0) continue;

                    _pendingDirection = new Vector2Int(x, y);
                }
            }

            if (_pendingDirection == Vector2Int.zero) return Vector2Int.zero;

            var dir = _pendingDirection;
            _pendingDirection = Vector2Int.zero;
            return dir;
        }
    }
}
