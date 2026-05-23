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
        private LineRenderer     _lr;
        private Transform        _hippo;
        private IDrawingState    _drawingState;
        private IGameLogger      _logger;
        private Vector3          _lastPos;
        private List<Vector3>    _positions = new List<Vector3>();
        private bool             _wasDrawing;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            _lr.useWorldSpace     = true;
            _lr.startColor        = Color.yellow;
            _lr.endColor          = Color.yellow;
            _lr.positionCount     = 0;
            _lr.material          = new Material(Shader.Find("Sprites/Default"));
            _lr.sortingOrder      = 1;
            _lr.numCornerVertices = 4;

            ApplyOnePixelWidth();
        }

        /// Считаем ширину 1 пикселя в мировых единицах через камеру.
        private void ApplyOnePixelWidth()
        {
            var cam = Camera.main;
            float width = cam != null
                ? cam.orthographicSize * 2f / Screen.height
                : 0.01f;

            _lr.startWidth = width;
            _lr.endWidth   = width;
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

            // Автоматически очищаем линию когда рисование заканчивается (дока, хит, заливка)
            if (_wasDrawing && !isDrawing)
                Clear();
            _wasDrawing = isDrawing;

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

        public void RestorePositions(IReadOnlyList<Vector3> worldPositions)
        {
            _positions.Clear();
            foreach (var p in worldPositions)
                _positions.Add(new Vector3(p.x, p.y, -0.1f));

            _lr.positionCount = _positions.Count;
            for (int i = 0; i < _positions.Count; i++)
                _lr.SetPosition(i, _positions[i]);

            _lastPos = _positions.Count > 0
                ? _positions[_positions.Count - 1]
                : Vector3.positiveInfinity;

            _logger?.Log($"[TrailLineRenderer] RestorePositions: {_positions.Count} точек");
        }
    }
}
