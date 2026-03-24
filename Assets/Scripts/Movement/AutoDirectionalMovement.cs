using System;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Movement
{
    public class AutoDirectionalMovement : IMovementBehaviour
    {
        public float Speed   { get; set; } = 5f;
        public bool  Stopped { get; private set; }

        public event Action<Vector2Int> OnDirectionChanged;

        public void Stop()
        {
            Stopped = true;
            Debug.Log("[AutoDirectionalMovement] Stop");
        }

        public void Resume(Transform target = null)
        {
            Stopped = false;
            Debug.Log("[AutoDirectionalMovement] Resume");
        }

        public void Tick(Transform target, ref Vector2Int currentDirection, Vector2Int inputDirection,
            Rect bounds, ICollisionService collision)
        {
            if (inputDirection != Vector2Int.zero)
            {
                if (DirectionGuard.IsReverse(currentDirection, inputDirection))
                {
                    // блок обратного
                }
                else if (inputDirection != currentDirection)
                {
                    currentDirection = inputDirection;
                    Stopped = false;
                    OnDirectionChanged?.Invoke(currentDirection);
                }
                else if (Stopped)
                {
                    Stopped = false;
                }
            }

            if (currentDirection == Vector2Int.zero || Stopped)
                return;

            Vector2 dir = ((Vector2)currentDirection).normalized;
            Vector3 next = target.position + (Vector3)(dir * Speed * Time.deltaTime);
            next.x = Mathf.Clamp(next.x, bounds.xMin, bounds.xMax);
            next.y = Mathf.Clamp(next.y, bounds.yMin, bounds.yMax);

            if (collision != null)
                next = collision.ClampToFree(target.position, next);

            target.position = next;
        }
    }
}
