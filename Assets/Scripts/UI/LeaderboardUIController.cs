using UnityEngine;
using YG;
using YG.Utils.LB;
using HippoGame.Interfaces;

namespace HippoGame.UI
{
    public class LeaderboardUIController : MonoBehaviour
    {
        [SerializeField] private GameObject        _panel;
        [SerializeField] private Transform         _slotsContainer;
        [SerializeField] private LeaderboardSlotUI _slotPrefab;

        private const string LB_NAME        = "RatingTable";
        private const int    QUANTITY_TOP    = 9;
        private const int    QUANTITY_AROUND = 0;
        private const string KEY_BEST_SCORE  = "hp_best_score";

        private IGameState _gameState;
        private int        _pendingScore;
        private bool       _isGameOver;

        // ─── Инициализация ───────────────────────────────────────────────

        public void Inject(IGameState gameState)
        {
            _gameState = gameState;
        }

        private void OnEnable()
        {
            YG2.onGetLeaderboard += OnLeaderboardReceived;
        }

        private void OnDisable()
        {
            YG2.onGetLeaderboard -= OnLeaderboardReceived;
        }

        private void Awake()
        {
            _panel.SetActive(false);
            ClearSlots();
        }

        // Вызывается при старте игры — показывает панель и грузит таблицу
        public void Initialize()
        {
            _isGameOver = false;
            _panel.SetActive(true);
            if (YG2.player.auth)
                YG2.GetLeaderboard(LB_NAME, QUANTITY_TOP, QUANTITY_AROUND, "nonePhoto");
        }

        // ─── Game Over ───────────────────────────────────────────────────

        // Вызывается после проигрыша
        public void Show()
        {
            _isGameOver   = true;
            _pendingScore = _gameState?.Score ?? 0;

            if (YG2.player.auth)
            {
                if (SaveBestScoreLocally(_pendingScore))
                {
                    YG2.SetLeaderboard(LB_NAME, _pendingScore);
                    Debug.Log($"[Leaderboard] Новый рекорд {_pendingScore} — отправили в таблицу");
                }
                else
                {
                    Debug.Log($"[Leaderboard] Счёт {_pendingScore} не лучше рекорда — не отправляем");
                }

                ClearSlots();
                YG2.GetLeaderboard(LB_NAME, QUANTITY_TOP, QUANTITY_AROUND, "nonePhoto");
            }
            else
            {
                Debug.Log("[Leaderboard] Игрок не авторизован — таблица пропущена");
                AppendPlayerSlot();
            }
        }

        // Возвращает true если новый счёт лучше сохранённого
        private bool SaveBestScoreLocally(int score)
        {
            int saved = YG2.iPlatform.GetInt(KEY_BEST_SCORE, 0);
            if (score > saved)
            {
                YG2.iPlatform.SetInt(KEY_BEST_SCORE, score);
                if (YG2.player.auth)
                    YG2.SaveProgress();
                return true;
            }
            return false;
        }

        private int GetBestScoreLocally() => YG2.iPlatform.GetInt(KEY_BEST_SCORE, 0);

        public void Hide()
        {
            _panel.SetActive(false);
        }

        // ─── Получение данных ────────────────────────────────────────────

        private void OnLeaderboardReceived(LBData lbData)
        {
            if (lbData.technoName != LB_NAME)
                return;

            ClearSlots();

            // Рендерим топ-9
            bool playerFound = false;
            string myName    = YG2.player.name;

            for (int i = 0; i < lbData.players.Length; i++)
            {
                var player = lbData.players[i];
                var slot   = Instantiate(_slotPrefab, _slotsContainer);
                slot.Setup(player.rank, player.name, player.score);

                if (player.name == myName)
                    playerFound = true;
            }

            // Добавляем слот только если нас нет в топ-9
            if (!playerFound)
                AppendPlayerSlot();
        }

        // Всегда добавляем слот игрока последним (10-е место визуально)
        private void AppendPlayerSlot()
        {
            string name        = YG2.player.name;
            int    currentScore = _isGameOver ? _pendingScore : (_gameState?.Score ?? 0);
            int    score       = Mathf.Max(currentScore, GetBestScoreLocally());

            var slot = Instantiate(_slotPrefab, _slotsContainer);
            slot.Setup(0, name, score);
            Debug.Log($"[Leaderboard] Добавлен слот игрока: {name} | {score}");
        }

        // ─── Утилиты ─────────────────────────────────────────────────────

        private void ClearSlots()
        {
            for (int i = _slotsContainer.childCount - 1; i >= 0; i--)
                Destroy(_slotsContainer.GetChild(i).gameObject);
        }
    }
}
