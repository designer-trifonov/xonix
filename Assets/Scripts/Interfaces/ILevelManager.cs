using System;

namespace HippoGame.Interfaces
{
    public interface ILevelManager
    {
        float LastFillPct { get; }

        event Action OnGameOver;
        event Action OnLevelComplete;
        event Action OnLevelStarted;

        void Initialize();
        void RestoreLastFillPct(float pct);
        void SetRespawnAction(Action respawn);
        void ClearFieldVisuals();
        void RestartFromLevel1();
        void ContinueAfterAd();
    }
}
