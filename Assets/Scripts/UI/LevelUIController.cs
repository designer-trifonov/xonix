using TMPro;
using UnityEngine;
using HippoGame.Interfaces;
using YG;

namespace HippoGame.UI
{
    public class LevelUIController : MonoBehaviour, IInitializable
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string   _labelRu = "Уровень";
        [SerializeField] private string   _labelEn = "Level";

        private IGameState            _state;
        private bool                  _initialized;
        private System.Action<string> _langHandler;

        public void Inject(IGameState state) => _state = state;

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            if (_text == null) _text = GameObject.Find("Level_Text (TMP)")?.GetComponent<TMP_Text>();
            _state.OnChanged += Refresh;
            _langHandler = _ => Refresh();
            YG2.onSwitchLang += _langHandler;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_state != null)      _state.OnChanged -= Refresh;
            if (_langHandler != null) YG2.onSwitchLang -= _langHandler;
        }

        private void Refresh()
        {
            if (_text == null || _state == null) return;
            var label = YG2.lang == "en" ? _labelEn : _labelRu;
            _text.text = $"{label} {_state.Level}";
        }
    }
}
