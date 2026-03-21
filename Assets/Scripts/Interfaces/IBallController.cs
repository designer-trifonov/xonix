using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IBallController
    {
        bool    IsAlive  { get; }
        Vector3 Position { get; }
        void    Kill();
        void    SetSpeed(float speed);
    }
}
