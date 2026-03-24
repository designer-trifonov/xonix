using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HippoGame.Interfaces;
using HippoGame.Ads;

namespace HippoGame.UI
{
    /// Магазин бустов. Каждый буст открывается просмотром rewarded рекламы.
    public class ShopUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private Button     _openShopButton;

        [Header("Boosts")]
        [SerializeField] private Button _addLifeButton;
        [SerializeField] private Button _removeBallButton;
        [SerializeField] private Button _slowBallsButton;

        [Header("Ad Labels")]
        [SerializeField] private TMP_Text _addLifeLabel;
        [SerializeField] private TMP_Text _removeBallLabel;
        [SerializeField] private TMP_Text _slowBallsLabel;

        private IGameState       _gameState;
        private IBallSpawner     _ballSpawner;
        private YandexAdsService _ads;

        private bool _adInProgress;

        public void Inject(IGameState gameState, IBallSpawner ballSpawner)
        {
            _gameState   = gameState;
            _ballSpawner = ballSpawner;
            _ads         = FindObjectOfType<YandexAdsService>();
        }

        private void Awake()
        {
            _shopPanel.SetActive(false);
            _openShopButton.onClick.AddListener(ToggleShop);
            _addLifeButton.onClick.AddListener(()    => WatchAd(ApplyAddLife,    _addLifeLabel,   "+1 жизнь"));
            _removeBallButton.onClick.AddListener(() => WatchAd(ApplyRemoveBall, _removeBallLabel, "-1 шар"));
            _slowBallsButton.onClick.AddListener(()  => WatchAd(ApplySlowBalls,  _slowBallsLabel,  "-20% скорость"));
        }

        private void ToggleShop()
        {
            bool next = !_shopPanel.activeSelf;
            _shopPanel.SetActive(next);
            Time.timeScale = next ? 0f : 1f;
        }

        private void WatchAd(System.Action onComplete, TMP_Text label, string boostName)
        {
            if (_adInProgress) return;
            _adInProgress = true;
            SetAllButtonsInteractable(false);
            if (label != null) label.text = "Загрузка...";

            _ads.ShowRewarded(
                onSuccess: () =>
                {
                    if (label != null) label.text = boostName;
                    onComplete?.Invoke();
                    Debug.Log($"[ShopUIController] Буст применён: {boostName}");
                    SetAllButtonsInteractable(true);
                    _adInProgress = false;
                    _shopPanel.SetActive(false);
                    Time.timeScale = 1f;
                },
                onFailed: () =>
                {
                    if (label != null) label.text = boostName;
                    Debug.Log("[ShopUIController] Реклама не досмотрена — буст не выдан");
                    SetAllButtonsInteractable(true);
                    _adInProgress = false;
                }
            );
        }

        private void ApplyAddLife()    => _gameState.AddLife();
        private void ApplySlowBalls()  => _ballSpawner.SlowBalls(0.8f);

        private void ApplyRemoveBall()
        {
            if (!_ballSpawner.RemoveOneBall())
                Debug.Log("[ShopUIController] RemoveOneBall: уже минимум шаров");
        }

        private void SetAllButtonsInteractable(bool value)
        {
            _addLifeButton.interactable    = value;
            _removeBallButton.interactable = value;
            _slowBallsButton.interactable  = value;
        }
    }
}
