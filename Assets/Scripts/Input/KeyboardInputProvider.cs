using UnityEngine;
using UnityEngine.InputSystem;
using HippoGame.Interfaces;

namespace HippoGame.Input
{
    /// Читает WASD и стрелки, возвращает направление движения.
    public class KeyboardInputProvider : IInputProvider
    {
        public Vector2 GetDirection()
        {
            var kb = Keyboard.current;
            if (kb == null) return Vector2.zero;

            if (kb.downArrowKey.wasPressedThisFrame  || kb.sKey.wasPressedThisFrame) return Vector2.down;
            if (kb.upArrowKey.wasPressedThisFrame    || kb.wKey.wasPressedThisFrame) return Vector2.up;
            if (kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame) return Vector2.left;
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) return Vector2.right;

            return Vector2.zero;
        }
    }
}
