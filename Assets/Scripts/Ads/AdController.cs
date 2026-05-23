using System;
using UnityEngine;
using HippoGame.Interfaces;

#if YandexGamesPlatform_yg
using YG;
using YG.Insides;
#endif

namespace HippoGame.Ads
{
    public class AdController : MonoBehaviour, IAdService
    {
        public bool IsAdShowing
        {
            get
            {
#if YandexGamesPlatform_yg
                return YG2.nowAdsShow;
#else
                return false;
#endif
            }
        }

        public event Action OnInterstitialClosed;
        public event Action OnRewardedOpen;
        public event Action OnRewardedClose;
        public event Action OnRewardedError;

        private bool _interstitialClosedFired;

        private void OnEnable()
        {
#if YandexGamesPlatform_yg
            YG2.onOpenInterAdv         += OnInterOpen;
            YG2.onCloseInterAdv        += OnInterClose;
            YG2.onCloseInterAdvWasShow += OnInterWasShow;
            YG2.onErrorInterAdv        += OnInterError;
            YG2.onOpenRewardedAdv      += OnRewardOpen;
            YG2.onCloseRewardedAdv     += OnRewardClose;
            YG2.onErrorRewardedAdv     += OnRewardError;
#endif
        }

        private void OnDisable()
        {
#if YandexGamesPlatform_yg
            YG2.onOpenInterAdv         -= OnInterOpen;
            YG2.onCloseInterAdv        -= OnInterClose;
            YG2.onCloseInterAdvWasShow -= OnInterWasShow;
            YG2.onErrorInterAdv        -= OnInterError;
            YG2.onOpenRewardedAdv      -= OnRewardOpen;
            YG2.onCloseRewardedAdv     -= OnRewardClose;
            YG2.onErrorRewardedAdv     -= OnRewardError;
#endif
        }

#if YandexGamesPlatform_yg
        private void OnInterOpen()              { _interstitialClosedFired = false; }
        private void OnInterClose()             { FireInterstitialClosed(); }
        private void OnInterWasShow(bool shown) { FireInterstitialClosed(); }
        private void OnInterError()
        {
            // На мобилках interstitial может не открыться — всё равно сбрасываем флаг
            YG2.nowInterAdv = false;
            FireInterstitialClosed();
        }

        private void FireInterstitialClosed()
        {
            if (_interstitialClosedFired) return;
            _interstitialClosedFired = true;
            OnInterstitialClosed?.Invoke();
        }

        private void OnRewardOpen()  => OnRewardedOpen?.Invoke();
        private void OnRewardClose() => OnRewardedClose?.Invoke();
        private void OnRewardError()
        {
            // На мобилках флаг может зависнуть — сбрасываем
            YG2.nowRewardAdv = false;
            OnRewardedError?.Invoke();
        }
#endif

        public void ShowInterstitial()
        {
#if YandexGamesPlatform_yg
            // Сбрасываем зависший флаг на случай если прошлый interstital не закрылся корректно
            if (YG2.nowInterAdv)
            {
                Debug.LogWarning("[AdController] nowInterAdv stuck=true — сбрасываем перед показом");
                YG2.nowInterAdv = false;
            }
            YGInsides.ResetTimerInterAdv();
            YG2.optionalPlatform.FirstInterAdvShow();
#else
            Debug.Log("[AdController] ShowInterstitial (stub)");
            OnInterstitialClosed?.Invoke();
#endif
        }

        public void ShowRewarded(string id, Action onReward)
        {
#if YandexGamesPlatform_yg
            // Сбрасываем зависший флаг interstitial — иначе rewarded не запустится
            if (YG2.nowInterAdv)
            {
                Debug.LogWarning("[AdController] nowInterAdv stuck=true — сбрасываем перед rewarded");
                YG2.nowInterAdv = false;
            }

            // Используем событие onRewardAdv (надёжнее callback на мобилках)
            void OnRewardEvent(string rewardId)
            {
                if (rewardId != id) return;
                YG2.onRewardAdv -= OnRewardEvent;
                Debug.Log($"[AdController] onRewardAdv '{rewardId}' — награда");
                onReward?.Invoke();
            }

            YG2.onRewardAdv += OnRewardEvent;
            YG2.RewardedAdvShow(id);
#else
            Debug.Log($"[AdController] ShowRewarded '{id}' (stub) — награда выдана");
            onReward?.Invoke();
#endif
        }
    }
}
