using System;
using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Interfaces
{
    /// Контракт спаунера шаров:
    /// LevelManager управляет шарами только через этот интерфейс.
    public interface IBallSpawner
    {
        event Action OnAnyBallBounce;

        void                                    SpawnBalls(int count, float speed);
        void                                    SpawnBallsAtData(IReadOnlyList<(Vector2 pos, Vector2 dir)> data, float speed);
        int                                     CheckBallsAfterFill();
        void                                    ClearBalls();
        bool                                    RemoveOneBall();
        void                                    SlowBalls(float factor);
        void                                    SetBallsVisible(bool visible);
        IReadOnlyList<Vector2>                  GetPositions();
        IReadOnlyList<(Vector2 pos, Vector2 dir)> GetBallsData();
        float                                   GetCurrentSpeed();
    }
}
