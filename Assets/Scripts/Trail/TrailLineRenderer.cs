using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Trail
{
    /// Рисует хвост гиппо через LineRenderer.
    /// IGridService инжектится один раз — конвертирует клетки в мировые координаты.
    [RequireComponent(typeof(LineRenderer))]
    public class TrailLineRenderer : MonoBehaviour, ITrailVisualizer
    {
        private const float LineWidth = 0.05f;

        private LineRenderer  _lr;
        private IGridService  _grid;

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
            Debug.Log("[TrailLineRenderer] Awake — LineRenderer инициализирован");
        }

        public void Inject(IGridService grid)
        {
            _grid = grid;
            Debug.Log("[TrailLineRenderer] Inject — IGridService получен");
        }

        public void Redraw(IReadOnlyList<Vector2Int> points)
        {
            if (_grid == null) return;

            _lr.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 world = _grid.CellToWorld(points[i]);
                _lr.SetPosition(i, new Vector3(world.x, world.y, -0.1f));
            }
        }

        public void RedrawWithPreview(IReadOnlyList<Vector2Int> points, Vector2Int previewCell)
        {
            if (_grid == null) return;

            _lr.positionCount = points.Count + 1;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 world = _grid.CellToWorld(points[i]);
                _lr.SetPosition(i, new Vector3(world.x, world.y, -0.1f));
            }
            Vector2 preview = _grid.CellToWorld(previewCell);
            _lr.SetPosition(points.Count, new Vector3(preview.x, preview.y, -0.1f));
        }

        public void Clear()
        {
            _lr.positionCount = 0;
            Debug.Log("[TrailLineRenderer] Clear");
        }
    }
}
