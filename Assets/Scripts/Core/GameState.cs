using System;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Core
{
    public class GameState : IGameState
    {
        public int Lives { get; private set; }
        public int Score { get; private set; } = 0;
        public int Level { get; private set; } = 1;

        private LevelConfig  _config;

        public event Action OnChanged;

        public float RequiredFillPercent => _config.BaseFillPercent     + (Level - 1) * _config.FillPercentPerLevel;
        public float HippoSpeed          => _config.BaseHippoSpeed      + (Level - 1) * _config.HippoSpeedPerLevel;
        public float BallSpeed           => _config.BaseBallSpeed       + (Level - 1) * _config.BallSpeedPerLevel;
        public int   BallCount           => Level;

        public GameState(LevelConfig config)
        {
            _config = config;
            Lives   = config.StartLives;
        }

        public void ZoneFilled(float fillDeltaPct)
        {
            int points = Mathf.RoundToInt(fillDeltaPct / 100f * _config.PointsPerLevel);
            Score += points;
            Debug.Log($"[GameState] ZoneFilled +{fillDeltaPct:F1}% → +{points} pts → Score={Score}");
            OnChanged?.Invoke();
        }

        public void AddLife()
        {
            Lives++;
            Debug.Log($"[GameState] AddLife → Lives={Lives}");
            OnChanged?.Invoke();
        }

        public void LoseLife()
        {
            Lives = Mathf.Max(0, Lives - 1);
            Debug.Log($"[GameState] LoseLife → Lives={Lives}");
            OnChanged?.Invoke();
        }

        public void NextLevel()
        {
            Level++;
            Debug.Log($"[GameState] NextLevel → Level={Level} | target={RequiredFillPercent:F0}% hippo={HippoSpeed:F1} ball={BallSpeed:F1} balls={BallCount}");
            OnChanged?.Invoke();
        }

        public void RestoreLives()
        {
            Lives = _config.StartLives;
            Debug.Log($"[GameState] RestoreLives → Lives={Lives}");
            OnChanged?.Invoke();
        }

        public void Reset()
        {
            Lives = _config.StartLives;
            Score = 0;
            Level = 1;
            _zonesFilledTotal = 0;
            Debug.Log("[GameState] Reset");
            OnChanged?.Invoke();
        }
    }
}
