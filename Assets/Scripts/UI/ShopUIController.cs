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
            _addLifeButton.onClick.AddListener(()    => WatchAd(ApplyAddLife,    "+1 жизнь"));
            _removeBallButton.onClick.AddListener(() => WatchAd(ApplyRemoveBall, "-1 шар"));
            _slowBallsButton.onClick.AddListener(()  => WatchAd(ApplySlowBalls,  "-20% скорость"));
        }

        private void ToggleShop()
        {
            bool next = !_shopPanel.activeSelf;
            _shopPanel.SetActive(next);
            Time.timeScale = next ? 0f : 1f;
            SetBallsVisible(!next);
        }

        private void SetBallsVisible(bool visible)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag("Ball"))
            {
                var r = go.GetComponent<Renderer>();
                if (r != null) r.enabled = visible;
            }
        }

        private void WatchAd(System.Action onComplete, string boostName)
        {
            if (_adInProgress) return;

            // Нет рекламного сервиса — даём буст бесплатно (dev mode)
            if (_ads == null)
            {
                onComplete?.Invoke();
                Debug.Log($"[ShopUIController] DEV: буст бесплатно — {boostName}");
                _shopPanel.SetActive(false);
                Time.timeScale = 1f;
                return;
            }

            _adInProgress = true;
            SetAllButtonsInteractable(false);

            _ads.ShowRewarded(
                onSuccess: () =>
                {
                    onComplete?.Invoke();
                    Debug.Log($"[ShopUIController] Буст применён: {boostName}");
                    SetAllButtonsInteractable(true);
                    _adInProgress = false;
                    _shopPanel.SetActive(false);
                    Time.timeScale = 1f;
                },
                onFailed: () =>
                {
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
