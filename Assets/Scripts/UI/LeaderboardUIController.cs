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

        public void Inject(IGameState gameState) => _gameState = gameState;

        private void OnEnable()  => YG2.onGetLeaderboard += OnLeaderboardReceived;
        private void OnDisable() => YG2.onGetLeaderboard -= OnLeaderboardReceived;

        private void Awake()
        {
            _panel.SetActive(false);
            ClearSlots();
        }

        /// Вызывается при старте и рестарте игры.
        public void Initialize()
        {
            _isGameOver   = false;
            _pendingScore = 0;
            _panel.SetActive(true);

            if (YG2.player.auth)
            {
                // Не чистим — старые слоты остаются пока грузятся новые данные
                // OnLeaderboardReceived сам почистит и перерисует
                YG2.GetLeaderboard(LB_NAME, QUANTITY_TOP, QUANTITY_AROUND, "nonePhoto");
            }
            else
            {
                // Не авторизован: всегда показываем лучший счёт
                RefreshUnauthSlot();
            }
        }

        // ─── Game Over ───────────────────────────────────────────────────

        public void Show()
        {
            _isGameOver   = true;
            _pendingScore = _gameState?.Score ?? 0;
            SaveBestScoreLocally(_pendingScore);

            if (YG2.player.auth)
            {
                YG2.SetLeaderboard(LB_NAME, GetBestScoreLocally());
                ClearSlots();
                YG2.GetLeaderboard(LB_NAME, QUANTITY_TOP, QUANTITY_AROUND, "nonePhoto");
                Debug.Log($"[Leaderboard] Game over | счёт={_pendingScore} рекорд={GetBestScoreLocally()}");
            }
            else
            {
                RefreshUnauthSlot();
                Debug.Log($"[Leaderboard] Не авторизован | рекорд={GetBestScoreLocally()}");
            }
        }

        public void Hide() => _panel.SetActive(false);

        // ─── Получение данных (только авторизованные) ────────────────────

        private void OnLeaderboardReceived(LBData lbData)
        {
            if (lbData.technoName != LB_NAME) return;

            ClearSlots();

            bool   playerFound = false;
            string myName      = YG2.player.name;

            for (int i = 0; i < lbData.players.Length; i++)
            {
                var  player    = lbData.players[i];
                if (player.score <= 0) continue;

                bool isMe      = player.name == myName;
                int  showScore = isMe
                    ? Mathf.Max(player.score, GetBestScoreLocally())
                    : player.score;

                Instantiate(_slotPrefab, _slotsContainer)
                    .Setup(player.rank, player.name, showScore);

                if (isMe) playerFound = true;
            }

            // Своего слота нет в топ — вешаем последним с лучшим счётом
            if (!playerFound)
                AppendPlayerSlot();
        }

        // ─── Неавторизованный ────────────────────────────────────────────

        /// Очищает и показывает единственный слот с лучшим счётом игрока.
        private void RefreshUnauthSlot()
        {
            ClearSlots();
            int best = GetBestScoreLocally();
            if (best > 0)
                AppendPlayerSlot();
        }

        // ─── Утилиты ─────────────────────────────────────────────────────

        private void AppendPlayerSlot()
        {
            int score = Mathf.Max(_pendingScore, GetBestScoreLocally());
            Instantiate(_slotPrefab, _slotsContainer)
                .Setup(0, YG2.player.name, score);
            Debug.Log($"[Leaderboard] Слот игрока: {YG2.player.name} | {score}");
        }

        private void SaveBestScoreLocally(int score)
        {
            if (score <= GetBestScoreLocally()) return;
            YG2.iPlatform.SetInt(KEY_BEST_SCORE, score);
            YG2.SaveProgress(); // локально всегда, в облако только если авторизован
        }

        private int GetBestScoreLocally() => YG2.iPlatform.GetInt(KEY_BEST_SCORE, 0);

        private void ClearSlots()
        {
            for (int i = _slotsContainer.childCount - 1; i >= 0; i--)
                Destroy(_slotsContainer.GetChild(i).gameObject);
        }
    }
}
