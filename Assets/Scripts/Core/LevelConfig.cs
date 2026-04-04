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

        [Header("Ball Speed (константа, не растёт с уровнем)")]
        public float BallSpeed = 2f;

        [Header("Score")]
        public int PointsPerLevel = 10000;

        public float GetRequiredFillPercent(Difficulty d) => d switch
        {
            Difficulty.Easy   => EasyFillPercent,
            Difficulty.Medium => MediumFillPercent,
            Difficulty.Hard   => HardFillPercent,
            _                 => MediumFillPercent
        };

        // Скорость бегемота = скорость арбузов, обе константы
        public float GetHippoSpeed(int level) => BallSpeed;
        public float GetBallSpeed(int level)  => BallSpeed;
        public int   GetBallCount(int level)  => level;
    }
}
