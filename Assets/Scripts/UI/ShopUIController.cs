using UnityEngine;
using UnityEngine.UI;
using HippoGame.Interfaces;

namespace HippoGame.UI
{
    public class ShopUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private Button     _openShopButton;
        [SerializeField] private Button     _closeShopButton;

        [Header("Boosts")]
        [SerializeField] private Button _addLifeButton;
        [SerializeField] private Button _removeBallButton;
        [SerializeField] private Button _slowBallsButton;

        private IGameState    _gameState;
        private IBallSpawner  _ballSpawner;
        private IAdService    _adService;
        private IPauseService _pause;
        private bool          _adInProgress;
        private bool          _isOpen;

        public void Inject(IGameState gameState, IBallSpawner ballSpawner, IAdService adService, IPauseService pause)
        {
            _gameState   = gameState;
            _ballSpawner = ballSpawner;
            _adService   = adService;
            _pause       = pause;
        }

        private void Awake()
        {
            _shopPanel.SetActive(false);
            _openShopButton.onClick.AddListener(ToggleShop);
            if (_closeShopButton != null)
                _closeShopButton.onClick.AddListener(CloseShop);
            _addLifeButton.onClick.AddListener(()    => WatchAd(ApplyAddLife,    "add_life"));
            _removeBallButton.onClick.AddListener(() => WatchAd(ApplyRemoveBall, "remove_ball"));
            _slowBallsButton.onClick.AddListener(()  => WatchAd(ApplySlowBalls,  "slow_balls"));
        }

        private void ToggleShop()
        {
            if (_isOpen) CloseShop();
            else         OpenShop();
        }

        private void OpenShop()
        {
            _isOpen = true;
            _shopPanel.SetActive(true);
            _pause?.Pause();
            _ballSpawner.SetBallsVisible(false);
        }

        private void CloseShop()
        {
            _isOpen = false;
            _shopPanel.SetActive(false);
            _pause?.Resume();
            _ballSpawner.SetBallsVisible(true);
            _adInProgress = false;
            SetAllButtonsInteractable(true);
        }

        private void WatchAd(System.Action onComplete, string advId)
        {
            if (_adInProgress || _adService.IsAdShowing)
            {
                Debug.Log($"[Shop] WatchAd '{advId}' — пропущено");
                return;
            }

            Debug.Log($"[Shop] WatchAd '{advId}' — запрос рекламы");
            _adInProgress = true;
            CloseShop();

            _adService.ShowRewarded(advId, () =>
            {
                Debug.Log($"[Shop] Награда получена за '{advId}'");
                onComplete?.Invoke();
            });
        }

        private void ApplyAddLife()   => _gameState.AddLife();
        private void ApplySlowBalls() => _ballSpawner.SlowBalls(0.8f);

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
