using UnityEngine;
using UnityEngine.InputSystem;
using HippoGame.Interfaces;

namespace HippoGame.Input
{
    /// Commit-on-release: направление фиксируется в момент первого отпускания клавиши.
    /// Chord накапливается пока жмёшь — фиксируется когда отпустил.
    public class KeyboardInputProvider : IInputProvider
    {
        private Vector2Int _chord;   // максимум зажатых клавиш за этот жест
        private bool       _active;  // идёт ли жест

        public Vector2Int GetDirection()
        {
            var kb = Keyboard.current;
            if (kb == null) return Vector2Int.zero;

            bool up    = kb.upArrowKey.isPressed    || kb.wKey.isPressed;
            bool down  = kb.downArrowKey.isPressed  || kb.sKey.isPressed;
            bool left  = kb.leftArrowKey.isPressed  || kb.aKey.isPressed;
            bool right = kb.rightArrowKey.isPressed || kb.dKey.isPressed;

            bool anyDown     = kb.upArrowKey.wasPressedThisFrame    || kb.wKey.wasPressedThisFrame
                            || kb.downArrowKey.wasPressedThisFrame  || kb.sKey.wasPressedThisFrame
                            || kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame
                            || kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;

            bool anyReleased = kb.upArrowKey.wasReleasedThisFrame    || kb.wKey.wasReleasedThisFrame
                            || kb.downArrowKey.wasReleasedThisFrame  || kb.sKey.wasReleasedThisFrame
                            || kb.leftArrowKey.wasReleasedThisFrame  || kb.aKey.wasReleasedThisFrame
                            || kb.rightArrowKey.wasReleasedThisFrame || kb.dKey.wasReleasedThisFrame;

            // Обновляем chord пока жест активен
            if (anyDown)
            {
                _active = true;
                int x = (right ? 1 : 0) - (left ? 1 : 0);
                int y = (up    ? 1 : 0) - (down ? 1 : 0);
                _chord = new Vector2Int(x, y);
            }

            // Commit на первом отпускании
            if (anyReleased && _active && _chord != Vector2Int.zero)
            {
                var dir = _chord;
                _chord  = Vector2Int.zero;
                _active = false;
                return dir;
            }

            return Vector2Int.zero;
        }
    }
}
