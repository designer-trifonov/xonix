using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Input
{
    /// Объединяет клавиатуру и тач одновременно.
    /// Приоритет: клавиатура → тач (если клава ничего не дала).
    public class CombinedInputProvider : IInputProvider
    {
        private readonly KeyboardInputProvider _keyboard;
        private readonly TouchInputProvider    _touch;

        public CombinedInputProvider()
        {
            _keyboard = new KeyboardInputProvider();
            _touch    = new TouchInputProvider();
        }

        public Vector2Int GetDirection()
        {
            var dir = _keyboard.GetDirection();
            if (dir != Vector2Int.zero) return dir;
            return _touch.GetDirection();
        }
    }
}
