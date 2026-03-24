using UnityEngine;

namespace HippoGame.Movement
{
    public static class DirectionGuard
    {
        /// Возвращает true если input — точно обратное направление к current.
        public static bool IsReverse(Vector2Int current, Vector2Int input)
            => current != Vector2Int.zero && input == -current;
    }
}
