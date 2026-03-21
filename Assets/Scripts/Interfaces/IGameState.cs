using System;

namespace HippoGame.Interfaces
{
    public interface IGameState
    {
        int   Lives { get; }
        int   Score { get; }
        int   Level { get; }
        float RequiredFillPercent { get; }
        float HippoSpeed          { get; }
        float BallSpeed           { get; }
        int   BallCount           { get; }

        event Action OnChanged;

        void ZoneFilled(float fillDeltaPct);
        void AddLife();
        void LoseLife();
        void NextLevel();
        void RestoreLives();
        void Reset();
    }
}
