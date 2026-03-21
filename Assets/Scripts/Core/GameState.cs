using System;
using UnityEngine;

namespace HippoGame.Core
{
    public class GameState
    {
        public int Lives      { get; private set; } = 3;
        public int Score      { get; private set; } = 0;
        public int Level      { get; private set; } = 1;

        private int _zonesFilledTotal = 0;

        public event Action OnChanged;

        public float RequiredFillPercent => 50f + (Level - 1) * 5f;
        public float HippoSpeed          => 3.5f + (Level - 1) * 0.5f;
        public float BallSpeed           => 2f   + (Level - 1) * 0.5f;
        public int   BallCount           => Level;

        public void ZoneFilled()
        {
            _zonesFilledTotal++;
            int points = 10000 + (_zonesFilledTotal - 1) * 5000;
            Score += points;
            Debug.Log($"[GameState] ZoneFilled #{_zonesFilledTotal} +{points} pts → Score={Score}");
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
            Lives = 3;
            Debug.Log("[GameState] RestoreLives → Lives=3");
            OnChanged?.Invoke();
        }

        public void Reset()
        {
            Lives = 3;
            Score = 0;
            Level = 1;
            _zonesFilledTotal = 0;
            Debug.Log("[GameState] Reset → Lives=3 Score=0 Level=1");
            OnChanged?.Invoke();
        }
    }
}
