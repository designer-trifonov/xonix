using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Grid
{
    /// Создаёт Quad + Texture2D и рисует состояние ячеек.
    /// Не знает о логике сетки — только рендерит что ей передают.
    public class GridTextureRenderer : MonoBehaviour, IGridRenderer
    {
        [SerializeField] private Texture2D _fillTexture;
        [SerializeField] private int       _pixelsPerUnit = 10;

        private Texture2D _gridTexture;
        private bool      _dirty;
        private int       _columns;
        private int       _rows;

        private static readonly Color BorderColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        public void Setup(int columns, int rows, Rect bounds)
        {
            _columns = columns;
            _rows    = rows;

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "GridQuad";
            quad.transform.position   = new Vector3(bounds.center.x, bounds.center.y, 0.5f);
            quad.transform.localScale = new Vector3(bounds.width, bounds.height, 1f);

            _gridTexture = new Texture2D(columns, rows, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            Renderer rend = quad.GetComponent<Renderer>();
            rend.material             = new Material(Shader.Find("Sprites/Default"));
            rend.material.mainTexture = _gridTexture;

            Debug.Log("[GridTextureRenderer] Quad создан, текстура привязана");
        }

        private void LateUpdate()
        {
            if (!_dirty) return;
            _gridTexture.Apply();
            _dirty = false;
        }

        public void SetPixel(int x, int y, CellState state)
        {
            if (_gridTexture == null) return;
            _gridTexture.SetPixel(x, y, CellColor(x, y, state));
            _dirty = true;
        }

        public void Refresh(IGridService grid)
        {
            if (_gridTexture == null) return;
            for (int x = 0; x < _columns; x++)
            for (int y = 0; y < _rows; y++)
                _gridTexture.SetPixel(x, y, CellColor(x, y, grid.GetCell(x, y)));
            _gridTexture.Apply();
        }

        private Color CellColor(int x, int y, CellState state)
        {
            if (x == 0 || x == _columns - 1 || y == 0 || y == _rows - 1)
                return BorderColor;
            return state switch
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
