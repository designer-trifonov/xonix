using UnityEngine;

namespace HippoGame.Interfaces
{
    /// Контракт взаимодействия шара с гиппо:
    /// шар спрашивает — можно ли бить, где трейл, что делать при попадании.
    public interface IBallInteractable
    {
        bool IsVulnerable { get; }
        bool IsNearTrail(Vector2 position, float threshold);
        void OnBallHit(Vector2 hitPosition);
    }
}
