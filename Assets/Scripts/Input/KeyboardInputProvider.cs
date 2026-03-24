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

            // Срабатываем только когда нажата новая клавиша
            bool anyNew = kb.upArrowKey.wasPressedThisFrame    || kb.wKey.wasPressedThisFrame
                       || kb.downArrowKey.wasPressedThisFrame  || kb.sKey.wasPressedThisFrame
                       || kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame
                       || kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;

            if (!anyNew) return Vector2.zero;

            // Читаем все зажатые клавиши — вектор собирается автоматически
            float x = ((kb.rightArrowKey.isPressed || kb.dKey.isPressed) ? 1f : 0f)
                    - ((kb.leftArrowKey.isPressed  || kb.aKey.isPressed) ? 1f : 0f);
            float y = ((kb.upArrowKey.isPressed    || kb.wKey.isPressed) ? 1f : 0f)
                    - ((kb.downArrowKey.isPressed  || kb.sKey.isPressed) ? 1f : 0f);

            if (x == 0f && y == 0f) return Vector2.zero;
            return new Vector2(x, y); // чистые компоненты: -1, 0, 1
        }
    }
}
