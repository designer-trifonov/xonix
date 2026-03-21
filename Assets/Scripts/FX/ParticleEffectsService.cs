using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.FX
{
    /// Воспроизводит партикл-эффекты в заданных точках мира.
    /// Каждый эффект — отдельный ParticleSystem-префаб, инстанцируется и сразу уничтожается.
    public class ParticleEffectsService : MonoBehaviour, IParticleService
    {
        [SerializeField] private ParticleSystem _dockEffect;
        [SerializeField] private ParticleSystem _ballHitEffect;

        public void PlayDock(Vector3 position)
        {
            if (_dockEffect == null)
            {
                Debug.LogWarning("[ParticleEffectsService] _dockEffect не назначен");
                return;
            }

            Play(_dockEffect, position);
            Debug.Log($"[ParticleEffectsService] Dock @ {position}");
        }

        public void PlayBallHit(Vector3 position)
        {
            if (_ballHitEffect == null)
            {
                Debug.LogWarning("[ParticleEffectsService] _ballHitEffect не назначен");
                return;
            }

            Play(_ballHitEffect, position);
            Debug.Log($"[ParticleEffectsService] BallHit @ {position}");
        }

        private static void Play(ParticleSystem prefab, Vector3 position)
        {
            ParticleSystem fx = Instantiate(prefab, position, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }
    }
}
