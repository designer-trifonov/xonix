using System.Collections.Generic;
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

        private LineRenderer     _lr;
        private Transform        _hippo;
        private IDrawingState    _drawingState;
        private IGameLogger      _logger;
        private Vector3          _lastPos;
        private List<Vector3>    _positions = new List<Vector3>();

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

        public void Inject(Transform hippo, IDrawingState drawingState = null, IGameLogger logger = null)
        {
            _hippo        = hippo;
            _drawingState = drawingState;
            _logger       = logger;
            _lastPos      = hippo.position;
            _logger?.Log("[TrailLineRenderer] Inject — hippo transform получен");
        }

        private void Update()
        {
            if (_hippo == null) return;

            bool isDrawing = _drawingState == null || _drawingState.IsDrawing;

            if (!isDrawing) return;

            Vector3 pos = _hippo.position;
            pos.z = -0.1f;

            if (pos != _lastPos)
            {
                _positions.Add(pos);
                _lr.positionCount = _positions.Count;
                _lr.SetPosition(_positions.Count - 1, pos);
                _lastPos = pos;
            }
        }

        public void Clear()
        {
            _positions.Clear();
            _lr.positionCount = 0;
            _lastPos = Vector3.positiveInfinity; // сброс, чтобы первая точка всегда добавилась
            _logger?.Log("[TRAIL CLEAR] причина: TrailLineRenderer.Clear() вызван явно");
        }
    }
}
