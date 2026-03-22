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

            bool up    = kb.upArrowKey.isPressed    || kb.wKey.isPressed;
            bool down  = kb.downArrowKey.isPressed  || kb.sKey.isPressed;
            bool left  = kb.leftArrowKey.isPressed  || kb.aKey.isPressed;
            bool right = kb.rightArrowKey.isPressed || kb.dKey.isPressed;

            bool upNew    = kb.upArrowKey.wasPressedThisFrame    || kb.wKey.wasPressedThisFrame;
            bool downNew  = kb.downArrowKey.wasPressedThisFrame  || kb.sKey.wasPressedThisFrame;
            bool leftNew  = kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame;
            bool rightNew = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;

            // Диагональ: одна клавиша нажата в этом кадре, другая уже зажата
            if ((upNew   && right) || (rightNew && up))   return new Vector2( 1,  1).normalized;
            if ((upNew   && left)  || (leftNew  && up))   return new Vector2(-1,  1).normalized;
            if ((downNew && right) || (rightNew && down)) return new Vector2( 1, -1).normalized;
            if ((downNew && left)  || (leftNew  && down)) return new Vector2(-1, -1).normalized;

            // Одиночные направления
            if (downNew)  return Vector2.down;
            if (upNew)    return Vector2.up;
            if (leftNew)  return Vector2.left;
            if (rightNew) return Vector2.right;

            return Vector2.zero;
        }
    }
}
