using UnityEngine;
using UnityEngine.InputSystem;
using HippoGame.Interfaces;

namespace HippoGame.Input
{
    /// Immediate с grace-буфером для диагоналей.
    /// При нажатии ждёт ~2 кадра, чтобы поймать вторую клавишу.
    public class KeyboardInputProvider : IInputProvider
    {
        private const float GraceTime = 0.04f; // ~2 кадра при 60fps

        private Vector2Int _lastCommitted;
        private Vector2Int _candidate;
        private float      _candidateAge;
        private bool       _hasPending;

        public Vector2Int GetDirection()
        {
            var kb = Keyboard.current;
            if (kb == null) return Vector2Int.zero;

            bool up    = kb.upArrowKey.isPressed    || kb.wKey.isPressed;
            bool down  = kb.downArrowKey.isPressed  || kb.sKey.isPressed;
            bool left  = kb.leftArrowKey.isPressed  || kb.aKey.isPressed;
            bool right = kb.rightArrowKey.isPressed || kb.dKey.isPressed;

            int x = (right ? 1 : 0) - (left ? 1 : 0);
            int y = (up    ? 1 : 0) - (down ? 1 : 0);
            var current = new Vector2Int(x, y);

            // Все клавиши отпущены — сбрасываем lastCommitted
            if (current == Vector2Int.zero)
                _lastCommitted = Vector2Int.zero;

            // Направление изменилось
            if (current != _candidate)
            {
                _candidate    = current;
                _candidateAge = 0f;
                _hasPending   = current != Vector2Int.zero && current != _lastCommitted;
            }

            if (!_hasPending) return Vector2Int.zero;

            _candidateAge += Time.deltaTime;

            // Кардинальное → ждём grace period (вдруг дожмут вторую клавишу)
            bool isSingleAxis = _candidate.x == 0 || _candidate.y == 0;
            if (isSingleAxis && _candidateAge < GraceTime)
                return Vector2Int.zero;

            // Диагональ или grace истёк → коммитим сразу
            _lastCommitted = _candidate;
            _hasPending    = false;
            return _candidate;
        }
    }
}
