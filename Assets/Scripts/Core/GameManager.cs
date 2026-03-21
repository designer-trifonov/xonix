using UnityEngine;
using HippoGame.Hippo;

namespace HippoGame.Core
{
    [ExecuteAlways]
    public class GameManager : MonoBehaviour
    {
        [Header("Game Zone")]
        [SerializeField] private Vector2 _zoneSize = new Vector2(11f, 7.6f);

        public Vector2 ZoneSize => _zoneSize;

        public static GameManager Instance { get; private set; }

        public void Register()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                HippoController hippo = FindObjectOfType<HippoController>();
                if (hippo != null)
                    hippo.SnapToSpawn();
            };
        }
#endif
    }
}
