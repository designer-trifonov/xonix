using TMPro;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.UI
{
    public class LevelUIController : MonoBehaviour, IInitializable
    {
        [SerializeField] private TMP_Text _text;

        private IGameState _state;

        public void Inject(IGameState state) => _state = state;

        public void Initialize()
        {
            if (_text == null) _text = GameObject.Find("Level_Text (TMP)")?.GetComponent<TMP_Text>();
            _state.OnChanged += Refresh;
            Refresh();
            Debug.Log("[LevelUIController] Initialize");
        }

        private void Refresh()
        {
            if (_text != null) _text.text = $"Уровень {_state.Level}";
        }
    }
}
