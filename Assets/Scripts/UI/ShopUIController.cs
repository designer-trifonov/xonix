using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HippoGame.Interfaces;

namespace HippoGame.UI
{
    /// Магазин бустов. Каждый буст открывается просмотром рекламы (пока мок).
    public class ShopUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private Button     _openShopButton;

        [Header("Boosts")]
        [SerializeField] private Button   _addLifeButton;
        [SerializeField] private Button   _removeBallButton;
        [SerializeField] private Button   _slowBallsButton;

        [Header("Ad Labels")]
        [SerializeField] private TMP_Text _addLifeLabel;
        [SerializeField] private TMP_Text _removeBallLabel;
        [SerializeField] private TMP_Text _slowBallsLabel;

        private IGameState   _gameState;
        private IBallSpawner _ballSpawner;

        private bool _adInProgress;

        public void Inject(IGameState gameState, IBallSpawner ballSpawner)
        {
            _gameState   = gameState;
            _ballSpawner = ballSpawner;
        }

        private void Awake()
        {
            _shopPanel.SetActive(false);
            _openShopButton.onClick.AddListener(ToggleShop);
            _addLifeButton.onClick.AddListener(() => StartCoroutine(WatchAd(ApplyAddLife,    _addLifeLabel,   "+1 жизнь")));
            _removeBallButton.onClick.AddListener(() => StartCoroutine(WatchAd(ApplyRemoveBall, _removeBallLabel, "-1 шар")));
            _slowBallsButton.onClick.AddListener(() => StartCoroutine(WatchAd(ApplySlowBalls,  _slowBallsLabel, "-20% скорость")));
        }

        private void ToggleShop()
        {
            bool next = !_shopPanel.activeSelf;
            _shopPanel.SetActive(next);
            Time.timeScale = next ? 0f : 1f;
            Debug.Log($"[ShopUIController] Магазин {(next ? "открыт" : "закрыт")}");
        }

        private IEnumerator WatchAd(System.Action onComplete, TMP_Text label, string boostName)
        {
            if (_adInProgress) yield break;
            _adInProgress = true;
            SetAllButtonsInteractable(false);

            string original = label != null ? label.text : boostName;

            for (int i = 5; i > 0; i--)
            {
                if (label != null) label.text = $"Реклама... {i}";
                yield return new WaitForSecondsRealtime(1f);
            }

            if (label != null) label.text = original;

            onComplete?.Invoke();
            Debug.Log($"[ShopUIController] Буст применён: {boostName}");

            SetAllButtonsInteractable(true);
            _adInProgress = false;

            // Закрываем магазин после буста
            _shopPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void ApplyAddLife()   => _gameState.AddLife();

        private void ApplyRemoveBall()
        {
            bool removed = _ballSpawner.RemoveOneBall();
            if (!removed)
                Debug.Log("[ShopUIController] RemoveOneBall: уже минимум шаров");
        }

        private void ApplySlowBalls() => _ballSpawner.SlowBalls(0.8f);

        private void SetAllButtonsInteractable(bool value)
        {
            _addLifeButton.interactable   = value;
            _removeBallButton.interactable = value;
            _slowBallsButton.interactable  = value;
        }
    }
}
