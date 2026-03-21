using TMPro;
using UnityEngine;
using HippoGame.Core;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.UI
{
    public class FillPercentUIController : MonoBehaviour, IInitializable
    {
        [SerializeField] private TMP_Text _text;

        private GameState    _state;
        private IGridService _grid;

        public void Inject(GameState state, IGridService grid)
        {
            _state = state;
            _grid  = grid;
        }

        public void Initialize()
        {
            if (_text == null)
                _text = GameObject.Find("Percent_Text")?.GetComponent<TMP_Text>();
        }

        private void LateUpdate()
        {
            if (_grid == null || _text == null) return;

            int total = _grid.Columns * _grid.Rows;
            if (total == 0) return;

            int filled = 0;
            for (int x = 0; x < _grid.Columns; x++)
            for (int y = 0; y < _grid.Rows; y++)
                if (_grid.GetCell(x, y) == CellState.Filled)
                    filled++;

            float pct = (float)filled / total * 100f;
            _text.text = $"{pct:F1}% / {_state.RequiredFillPercent:F0}%";
        }
    }
}
