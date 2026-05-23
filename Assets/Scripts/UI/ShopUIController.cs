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

        [Header("Confirm")]
        [SerializeField] private AdConfirmPopup _confirmPopup;

        private IGameState    _gameState;
        private IBallSpawner  _ballSpawner;
        private IAdService    _adService;
        private IPauseService _pause;
        private IGameLogger   _log;
        private bool          _adInProgress;
        private bool          _isOpen;

        public void Inject(IGameState gameState, IBallSpawner ballSpawner, IAdService adService, IPauseService pause, IGameLogger log = null)
        {
            _gameState   = gameState;
            _ballSpawner = ballSpawner;
            _adService   = adService;
            _pause       = pause;
            _log         = log;
        }

        private void Awake()
        {
            _shopPanel.SetActive(false);
            _openShopButton.onClick.AddListener(ToggleShop);
            if (_closeShopButton != null)
                _closeShopButton.onClick.AddListener(CloseShop);
            _addLifeButton.onClick.AddListener(()    => Confirm(() => WatchAd(ApplyAddLife,    "add_life")));
            _removeBallButton.onClick.AddListener(() => Confirm(() => WatchAd(ApplyRemoveBall, "remove_ball")));
            _slowBallsButton.onClick.AddListener(()  => Confirm(() => WatchAd(ApplySlowBalls,  "slow_balls")));
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

        private void Confirm(System.Action onConfirm)
        {
            _log?.Log($"[Shop] Confirm | confirmPopup={_confirmPopup != null}");
            if (_confirmPopup != null)
                _confirmPopup.Show(onConfirm);
            else
                onConfirm?.Invoke();
        }

        private void WatchAd(System.Action onComplete, string advId)
        {
            _log?.Log($"[Shop] WatchAd '{advId}' | adInProgress={_adInProgress} isAdShowing={_adService.IsAdShowing}");
            if (_adInProgress || _adService.IsAdShowing)
            {
                _log?.Log($"[Shop] WatchAd '{advId}' — пропущено (уже идёт)");
                return;
            }

            _log?.Log($"[Shop] WatchAd '{advId}' — запуск рекламы");
            _adInProgress = true;
            CloseShop();

            _adService.ShowRewarded(advId, () =>
            {
                _log?.Log($"[Shop] Награда получена за '{advId}'");
                onComplete?.Invoke();
            });
        }

        private void ApplyAddLife()   => _gameState.AddLife();
        private void ApplySlowBalls() => _ballSpawner.SlowBalls(0.8f);

        private void ApplyRemoveBall()
        {
            if (!_ballSpawner.RemoveOneBall())
                _log?.Log("[Shop] RemoveOneBall: уже минимум шаров");
        }

        private void SetAllButtonsInteractable(bool value)
        {
            _addLifeButton.interactable    = value;
            _removeBallButton.interactable = value;
            _slowBallsButton.interactable  = value;
        }
    }
}
