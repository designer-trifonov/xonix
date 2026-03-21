using System;
using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IMovementBehaviour
    {
        float Speed { get; set; }
        event Action<Vector2> OnDirectionChanged;
        void Tick(Transform transform, ref Vector2 currentDir, Vector2 inputDir, UnityEngine.Rect bounds, ICollisionService collision);
        void Stop();
        void Resume();
    }
}
