using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IBallController
    {
        bool    IsAlive   { get; }
        Vector3 Position  { get; }
        Vector2 Direction { get; }
        void    Kill();
        void    SetSpeed(float speed);
        void    SetVisible(bool visible);
    }
}
