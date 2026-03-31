using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Interfaces
{
    /// Контракт спаунера шаров:
    /// LevelManager управляет шарами только через этот интерфейс.
    public interface IBallSpawner
    {
        void                   SpawnBalls(int count, float speed);
        int                    CheckBallsAfterFill();
        void                   ClearBalls();
        bool                   RemoveOneBall();
        void                   SlowBalls(float factor);
        void                   SetBallsVisible(bool visible);
        IReadOnlyList<Vector2> GetPositions();
    }
}
