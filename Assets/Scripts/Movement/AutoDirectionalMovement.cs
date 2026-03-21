using System;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Movement
{
    /// Двигает объект по одной оси согласно вводу.
    /// Новое нажатие снимает стоп и меняет направление.
    public class AutoDirectionalMovement : IMovementBehaviour
    {
        public float Speed   { get; set; } = 5f;
        public bool  Stopped { get; private set; }

        /// Срабатывает когда игрок нажимает новую клавишу направления.
        public event Action<Vector2> OnDirectionChanged;

        public void Stop()
        {
            Stopped = true;
            Debug.Log("[AutoDirectionalMovement] Stop");
        }

        public void Resume()
        {
            Stopped = false;
            Debug.Log("[AutoDirectionalMovement] Resume");
        }

        public void Tick(Transform target, ref Vector2 currentDirection, Vector2 inputDirection,
            Rect bounds, ICollisionService collision)
        {
            if (inputDirection != Vector2.zero && inputDirection != currentDirection)
            {
                currentDirection = inputDirection;
                Stopped = false;
                OnDirectionChanged?.Invoke(currentDirection);
            }

            if (currentDirection == Vector2.zero || Stopped)
                return;

            Vector3 next = target.position + (Vector3)(currentDirection * Speed * Time.deltaTime);
            next.x = Mathf.Clamp(next.x, bounds.xMin, bounds.xMax);
            next.y = Mathf.Clamp(next.y, bounds.yMin, bounds.yMax);

            if (collision != null)
                next = collision.ClampToFree(target.position, next);

            target.position = next;
        }
    }
}
