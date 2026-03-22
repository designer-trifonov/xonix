using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Trail
{
    /// Тупой визуализатор хвоста гиппо.
    /// Правило одно: двигаемся — рисуем, стоим — не рисуем.
    /// Не знает ни о сетке, ни о зонах, ни о состоянии игры.
    [RequireComponent(typeof(LineRenderer))]
    public class TrailLineRenderer : MonoBehaviour, ITrailRenderer
    {
        private const float LineWidth = 0.10f;
        private const int   MaxPoints = 2000;

        private LineRenderer  _lr;
        private Transform     _hippo;
        private IDrawingState _drawingState;
        private Vector3       _lastPos;
        private Vector3[]     _positions = new Vector3[MaxPoints];
        private int           _count;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            _lr.useWorldSpace     = true;
            _lr.startWidth        = LineWidth;
            _lr.endWidth          = LineWidth;
            _lr.startColor        = Color.yellow;
            _lr.endColor          = Color.yellow;
            _lr.positionCount     = 0;
            _lr.material          = new Material(Shader.Find("Sprites/Default"));
            _lr.sortingOrder      = 1;
            _lr.numCornerVertices = 4;
        }

        public void Inject(Transform hippo, IDrawingState drawingState = null)
        {
            _hippo        = hippo;
            _drawingState = drawingState;
            _lastPos      = hippo.position;
            Debug.Log("[TrailLineRenderer] Inject — hippo transform получен");
        }

        private void Update()
        {
            if (_hippo == null) return;

            // Рисуем только когда активно рисование — не на стене, не после хита
            if (_drawingState != null && !_drawingState.IsDrawing)
            {
                if (_count > 0) Clear();
                return;
            }

            Vector3 pos = _hippo.position;
            pos.z = -0.1f;

            if (pos != _lastPos)
            {
                if (_count < MaxPoints)
                {
                    _positions[_count] = pos;
                    _count++;
                    _lr.positionCount = _count;
                    _lr.SetPosition(_count - 1, pos);
                }
                _lastPos = pos;
            }
        }

        public void Clear()
        {
            _count = 0;
            _lr.positionCount = 0;
            Debug.Log("[TrailLineRenderer] Clear");
        }
    }
}
