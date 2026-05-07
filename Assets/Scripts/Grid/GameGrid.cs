using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Zone;

namespace HippoGame.Grid
{
    /// Логика клеточной сетки: хранение состояний, координатные преобразования.
    /// Рендеринг делегирован GridTextureRenderer.
    public class GameGrid : MonoBehaviour, IGridService, IGridRenderer
    {
        [SerializeField] private int                 _pixelsPerUnit = 10;
        [SerializeField] private GameZone            _zone;
        [SerializeField] private GridTextureRenderer _renderer;

        private CellState[,] _cells;
        private int          _columns;
        private int          _rows;
        private Rect         _bounds;
        private float        _pixelSize;

        public int   Columns  => _columns;
        public int   Rows     => _rows;
        public Rect  Bounds   => _bounds;
        public float CellSize => _pixelSize;

        public void Initialize()
        {
            BuildGrid();
            _renderer?.Setup(_columns, _rows, _bounds);
            ResetCells();
        }

        private void BuildGrid()
        {
            Rect zoneBounds = _zone != null
                ? _zone.GetBounds()
                : new Rect(-5.5f, -3.8f, 11f, 7.6f);

            _bounds    = zoneBounds;
            _columns   = Mathf.Max(2, Mathf.RoundToInt(zoneBounds.width  * _pixelsPerUnit));
            _rows      = Mathf.Max(2, Mathf.RoundToInt(zoneBounds.height * _pixelsPerUnit));
            _pixelSize = 1f / _pixelsPerUnit;
            _cells     = new CellState[_columns, _rows];
            Debug.Log($"[GameGrid] Сетка {_columns}x{_rows} пикселей ({_pixelsPerUnit}ppu)");
        }

        public CellState GetCell(int x, int y) => _cells[x, y];

        public void SetCell(int x, int y, CellState state)
        {
            _cells[x, y] = state;
            _renderer?.SetPixel(x, y, state);
        }

        public Vector2Int WorldToCell(Vector2 worldPos)
        {
            int x = Mathf.Clamp(Mathf.FloorToInt((worldPos.x - _bounds.xMin) / _pixelSize), 0, _columns - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt((worldPos.y - _bounds.yMin) / _pixelSize), 0, _rows - 1);
            return new Vector2Int(x, y);
        }

        public Vector2 CellToWorld(Vector2Int cell)
        {
            float x = _bounds.xMin + (cell.x + 0.5f) * _pixelSize;
            float y = _bounds.yMin + (cell.y + 0.5f) * _pixelSize;
            return new Vector2(x, y);
        }

        public bool IsInBounds(Vector2Int cell) =>
            cell.x >= 0 && cell.x < _columns && cell.y >= 0 && cell.y < _rows;

        public bool IsEdge(Vector2Int cell) =>
            cell.x == 0 || cell.x == _columns - 1 || cell.y == 0 || cell.y == _rows - 1;

        public void ResetCells()
        {
            Debug.Log($"[GameGrid] ResetCells — очистка {_columns}x{_rows}");
            for (int x = 0; x < _columns; x++)
            for (int y = 0; y < _rows; y++)
            {
                bool edge = x == 0 || x == _columns - 1 || y == 0 || y == _rows - 1;
                SetCell(x, y, edge ? CellState.Filled : CellState.Empty);
            }
        }

        public void Refresh(IGridService grid) => _renderer?.Refresh(grid);

        public void RefreshRenderer() => _renderer?.Refresh(this);
    }
}
