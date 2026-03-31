using TMPro;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;
using static HippoGame.Grid.GridUtils;


namespace HippoGame.UI
{
    public class FillPercentUIController : MonoBehaviour, IInitializable
    {
        [SerializeField] private TMP_Text _text;

        private IGameState   _state;
        private IGridService _grid;

        public void Inject(IGameState state, IGridService grid)
        {
            _state = state;
            _grid  = grid;
        }

        public void Initialize()
        {
            if (_text == null) _text = GameObject.Find("Percent_Text (TMP)")?.GetComponent<TMP_Text>();
            Debug.Log("[FillPercentUIController] Initialize");
        }

        private void LateUpdate()
        {
            if (_grid == null || _text == null) return;
            float pct = CountFillPercent(_grid);
            _text.text = $"{pct:F1}% / {_state.RequiredFillPercent:F0}%";
        }
    }
}
