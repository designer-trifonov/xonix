using UnityEngine;

namespace HippoGame.Interfaces
{
    /// Сервис воспроизведения партикл-эффектов.
    public interface IParticleService
    {
        /// Гиппо состыковался со стеной — закрыл трейл.
        void PlayDock(Vector3 position);

        /// Шар попал в трейл или в гиппо.
        void PlayBallHit(Vector3 position);
    }
}
