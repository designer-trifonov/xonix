using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Grid
{
    /// Пиксельная сетка клеток + рендер состояния через Texture2D.
    public class GameGrid : MonoBehaviour, IGridService, IGridRenderer
    {
        [SerializeField] private Texture2D _fillTexture;
        [SerializeField] private int       _pixelsPerUnit = 10;
        [SerializeField] private Vector2   _zoneSize      = new Vector2(11f, 7.6f);

        private CellState[,] _cells;
        private Texture2D    _gridTexture;
        private bool         _dirty;
        private int          _columns;
        private int          _rows;
        private Rect         _bounds;
        private float        _pixelSize;

        public int   Columns  => _columns;
        public int   Rows     => _rows;
        public Rect  Bounds   => _bounds;
        public float CellSize => _pixelSize;

        private void Awake()
        {
            BuildGrid();
            CreateQuad();
        }

        private void LateUpdate()
        {
            if (!_dirty) return;
            _gridTexture.Apply();
            _dirty = false;
        }

        private void BuildGrid()
        {
            Vector2 zoneSize = _zoneSize;
            _bounds    = new Rect(-zoneSize.x / 2f, -zoneSize.y / 2f, zoneSize.x, zoneSize.y);
            _columns   = Mathf.Max(2, Mathf.RoundToInt(zoneSize.x * _pixelsPerUnit));
            _rows      = Mathf.Max(2, Mathf.RoundToInt(zoneSize.y * _pixelsPerUnit));
            _pixelSize = 1f / _pixelsPerUnit;
            _cells     = new CellState[_columns, _rows];
            Debug.Log($"[GameGrid] Сетка {_columns}x{_rows} пикселей ({_pixelsPerUnit}ppu)");
        }

        private void CreateQuad()
        {
            Vector2    zoneSize = _zoneSize;
            GameObject quad     = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "GridQuad";
            quad.transform.SetParent(transform);
            quad.transform.position   = new Vector3(0f, 0f, 0.5f);
            quad.transform.localScale = new Vector3(zoneSize.x, zoneSize.y, 1f);

            _gridTexture = new Texture2D(_columns, _rows, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            Renderer rend = quad.GetComponent<Renderer>();
            rend.material             = new Material(Shader.Find("Sprites/Default"));
            rend.material.mainTexture = _gridTexture;

            Refresh(this);
            Debug.Log("[GameGrid] Quad создан, текстура привязана");
        }

        public CellState GetCell(int x, int y) => _cells[x, y];

        public void SetCell(int x, int y, CellState state)
        {
            _cells[x, y] = state;
            _gridTexture.SetPixel(x, y, CellColor(x, y));
            _dirty = true;
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
                SetCell(x, y, CellState.Empty);
        }

        public void Refresh(IGridService grid)
        {
            if (_gridTexture == null) return;
            for (int x = 0; x < _columns; x++)
            for (int y = 0; y < _rows; y++)
                _gridTexture.SetPixel(x, y, CellColor(x, y));
            _gridTexture.Apply();
        }

        private static readonly Color BorderColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        private Color CellColor(int x, int y)
        {
            if (x == 0 || x == _columns - 1 || y == 0 || y == _rows - 1)
                return BorderColor;
            return _cells[x, y] switch
            {
                CellState.Filled => FillColor(x, y),
                CellState.Trail  => Color.yellow,
                _                => Color.clear
            };
        }

        private Color FillColor(int x, int y)
        {
            if (_fillTexture == null || !_fillTexture.isReadable)
                return new Color(0.2f, 0.6f, 1f);
            return _fillTexture.GetPixelBilinear((float)x / _columns, (float)y / _rows);
        }
    }
}
