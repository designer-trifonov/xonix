using UnityEngine;

namespace HippoGame.Core
{
    public enum Difficulty { Easy, Medium, Hard }

    [CreateAssetMenu(menuName = "HippoGame/LevelConfig", fileName = "LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Lives")]
        public int StartLives = 3;

        [Header("Fill % по сложности")]
        public float EasyFillPercent   = 60f;
        public float MediumFillPercent = 75f;
        public float HardFillPercent   = 85f;

        [Header("Ball Speed")]
        public float BaseBallSpeed     = 2f;
        public float BallSpeedPerLevel = 0.5f;

        [Header("Score")]
        public int PointsPerLevel = 10000;

        public float GetRequiredFillPercent(Difficulty d) => d switch
        {
            Difficulty.Easy   => EasyFillPercent,
            Difficulty.Medium => MediumFillPercent,
            Difficulty.Hard   => HardFillPercent,
            _                 => MediumFillPercent
        };

        // Скорость бегемота = скорость арбузов всегда
        public float GetHippoSpeed(int level) => GetBallSpeed(level);
        public float GetBallSpeed(int level)  => BaseBallSpeed + (level - 1) * BallSpeedPerLevel;
        public int   GetBallCount(int level)  => level;
    }
}
