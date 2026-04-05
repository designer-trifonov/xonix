using System;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Ads
{
    /// Показывает интерстишл при старте.
    /// Когда реклама закрыта — стреляет OnCompleted.
    public class GameStartAdHandler : MonoBehaviour
    {
        private IAdService _adService;

        public event Action OnCompleted;

        public void Inject(IAdService adService)
        {
            _adService = adService;
        }

        private void Start()
        {
            _adService.OnInterstitialClosed += HandleAdClosed;
            _adService.ShowInterstitial();
        }

        private void HandleAdClosed()
        {
            _adService.OnInterstitialClosed -= HandleAdClosed;
            Debug.Log("[GameStartAdHandler] Реклама закрыта → запускаем флоу");
            OnCompleted?.Invoke();
        }
    }
}
