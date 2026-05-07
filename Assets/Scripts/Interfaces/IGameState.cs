using System;
using HippoGame.Core;

namespace HippoGame.Interfaces
{
    public interface IGameState
    {
        int        Lives      { get; }
        int        Score      { get; }
        int        Level      { get; }
        Difficulty Difficulty { get; }
        float      RequiredFillPercent { get; }
        float      HippoSpeed          { get; }
        float      BallSpeed           { get; }
        int        BallCount           { get; }

        event Action OnChanged;

        void SetDifficulty(Difficulty d);
        void ZoneFilled(float fillDeltaPct);
        void AddLife();
        void LoseLife();
        void NextLevel();
        void RestoreLives();
        void Reset();
        void RestoreSession(int score, int level, int lives, Difficulty difficulty);
    }
}
