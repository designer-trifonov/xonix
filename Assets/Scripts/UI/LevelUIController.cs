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

        private IGameState _state;

        public void Inject(IGameState state) => _state = state;

        public void Initialize()
        {
            if (_text == null) _text = GameObject.Find("Level_Text (TMP)")?.GetComponent<TMP_Text>();
            _state.OnChanged += Refresh;
            YG2.onSwitchLang += _ => Refresh();
            Refresh();
        }

        private void Refresh()
        {
            if (_text == null) return;
            var label = YG2.lang == "en" ? _labelEn : _labelRu;
            _text.text = $"{label} {_state.Level}";
        }
    }
}
