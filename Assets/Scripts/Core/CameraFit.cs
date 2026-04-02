using UnityEngine;
using HippoGame.Zone;

namespace HippoGame.Core
{
    /// Подстраивает orthographicSize камеры так, чтобы игровое поле
    /// всегда было полностью видно — и в ландшафте, и в портрете.
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        [SerializeField] private GameZone _gameZone;
        [SerializeField] private float _padding = 0.5f;

        [Header("UI Layout")]
        [SerializeField] private RectTransform _header;
        [SerializeField] private float _headerPortraitOffsetY = -180f;

        private Camera _camera;
        private Vector2 _headerDefaultPos;
        private int _lastWidth;
        private int _lastHeight;

        public void Initialize()
        {
            Debug.Log("[CameraFit] Initialize");
            _camera = GetComponent<Camera>();
            if (_header != null)
                _headerDefaultPos = _header.anchoredPosition;
            Apply();
        }

        private void Update()
        {
            if (_camera == null) return;
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
                Apply();
        }

        private void Apply()
        {
            if (_camera == null) return;
            _lastWidth  = Screen.width;
            _lastHeight = Screen.height;

            Rect bounds = _gameZone.GetBounds();
            float zoneW = bounds.width  + _padding * 2f;
            float zoneH = bounds.height + _padding * 2f;

            float aspect = (float)Screen.width / Screen.height;

            // Минимальный orthographicSize чтобы влезла высота
            float fitH = zoneH / 2f;
            // Минимальный orthographicSize чтобы влезла ширина
            float fitW = (zoneW / 2f) / aspect;

            _camera.orthographicSize = Mathf.Max(fitH, fitW);

            if (_header != null)
            {
                bool isPortrait = Screen.height > Screen.width;
                _header.anchoredPosition = isPortrait
                    ? new Vector2(_headerDefaultPos.x, _headerDefaultPos.y + _headerPortraitOffsetY)
                    : _headerDefaultPos;
            }
        }
    }
}
