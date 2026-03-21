using UnityEngine;

namespace HippoGame.Core
{
    [CreateAssetMenu(menuName = "HippoGame/LevelConfig", fileName = "LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Lives")]
        public int StartLives = 3;

        [Header("Fill")]
        public float BaseFillPercent     = 50f;
        public float FillPercentPerLevel = 5f;

        [Header("Hippo Speed")]
        public float BaseHippoSpeed      = 3.5f;
        public float HippoSpeedPerLevel  = 0.5f;

        [Header("Ball Speed")]
        public float BaseBallSpeed       = 2f;
        public float BallSpeedPerLevel   = 0.5f;

        [Header("Score")]
        public int PointsPerLevel = 10000;
    }
}
