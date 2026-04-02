using UnityEngine;

namespace HippoGame.Core
{
    [CreateAssetMenu(menuName = "HippoGame/LevelConfig", fileName = "LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Lives")]
        public int StartLives = 3;

        [Header("Fill")]
        public float BaseFillPercent     = 75f;
        public float FillPercentPerLevel = 5f;

        [Header("Hippo Speed")]
        public float BaseHippoSpeed      = 3.5f;
        public float HippoSpeedPerLevel  = 0.5f;

        [Header("Score")]
        public int PointsPerLevel = 10000;

        public float GetRequiredFillPercent(int level) => BaseFillPercent     + (level - 1) * FillPercentPerLevel;
        public float GetHippoSpeed(int level)          => BaseHippoSpeed      + ((level - 1) / 5) * HippoSpeedPerLevel;
        public float GetBallSpeed(int level)           => GetHippoSpeed(level);
        public int   GetBallCount(int level)           => level;
    }
}
