using System;
using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IMovementBehaviour
    {
        float Speed   { get; set; }
        bool  Stopped { get; }
        event Action<Vector2Int> OnDirectionChanged;
        void Tick(Transform transform, ref Vector2Int currentDir, Vector2Int inputDir, Rect bounds, ICollisionService collision);
        void Stop();
        void Resume(Transform target = null);
    }
}
