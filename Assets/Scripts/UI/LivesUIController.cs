using TMPro;
using UnityEngine;
using HippoGame.Core;
using HippoGame.Interfaces;

namespace HippoGame.UI
{
    public class LivesUIController : MonoBehaviour, IInitializable
    {
        [SerializeField] private TMP_Text _text;

        private GameState _state;

        public void Inject(GameState state) => _state = state;

        public void Initialize()
        {
            if (_text == null)
                _text = GameObject.Find("Live_Text")?.GetComponent<TMP_Text>();

            _state.OnChanged += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            if (_text != null) _text.text = $"x{_state.Lives}";
        }
    }
}
