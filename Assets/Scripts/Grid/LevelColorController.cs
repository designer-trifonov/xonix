using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Grid
{
    /// Независимая система: меняет цвет заливки каждые 5 уровней.
    /// 10 цветов — цикл до уровня 50, далее по кругу.
    public class LevelColorController : MonoBehaviour
    {
        [SerializeField] private GridTextureRenderer _renderer;

        [SerializeField] private Color[] _palette = new Color[]
        {
            new Color(0.47f, 0.65f, 0.82f),  // 1–5   стальной синий
            new Color(0.55f, 0.72f, 0.55f),  // 6–10  шалфей
            new Color(0.80f, 0.58f, 0.55f),  // 11–15 пыльная роза
            new Color(0.85f, 0.70f, 0.42f),  // 16–20 янтарный
            new Color(0.40f, 0.68f, 0.68f),  // 21–25 приглушённый бирюзовый
            new Color(0.68f, 0.60f, 0.82f),  // 26–30 лаванда
            new Color(0.78f, 0.52f, 0.40f),  // 31–35 терракота
            new Color(0.88f, 0.65f, 0.55f),  // 36–40 персиковый
            new Color(0.65f, 0.70f, 0.40f),  // 41–45 оливковый
            new Color(0.55f, 0.58f, 0.80f),  // 46–50 голубично-синий
        };

        private IGameState   _gameState;
        private IGridService _grid;
        private int          _lastLevel = -1;

        public void Inject(IGameState state, IGridService grid)
        {
            _gameState           = state;
            _grid                = grid;
            _gameState.OnChanged += OnStateChanged;
            if (_renderer != null && _grid != null)
                _renderer.SetGridService(_grid);
        }

        public void Initialize() => ApplyColor();

        private void OnStateChanged()
        {
            if (_gameState.Level == _lastLevel) return;
            ApplyColor();
        }

        private void ApplyColor()
        {
            if (_renderer == null || _palette == null || _palette.Length == 0) return;
            _lastLevel = _gameState.Level;
            int index  = ((_gameState.Level - 1) / 5) % _palette.Length;
            _renderer.SetFillColor(_palette[index]);
            Debug.Log($"[LevelColorController] Уровень {_gameState.Level} → цвет [{index}]");
        }
    }
}
