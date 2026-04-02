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

        public event Action OnRewardedOpen;
        public event Action OnRewardedClose;
        public event Action OnRewardedError;

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
        private void OnInterOpen()              { Debug.Log("[AdController] Interstitial ОТКРЫТА"); }
        private void OnInterClose()             { Debug.Log("[AdController] Interstitial ЗАКРЫТА"); }
        private void OnInterWasShow(bool shown) { Debug.Log($"[AdController] Interstitial показана: {shown}"); }
        private void OnInterError()             { Debug.Log("[AdController] Interstitial ОШИБКА"); }

        private void OnRewardOpen()  { Debug.Log("[AdController] Rewarded ОТКРЫТА");  OnRewardedOpen?.Invoke(); }
        private void OnRewardClose() { Debug.Log("[AdController] Rewarded ЗАКРЫТА"); OnRewardedClose?.Invoke(); }
        private void OnRewardError() { Debug.Log("[AdController] Rewarded ОШИБКА");  OnRewardedError?.Invoke(); }
#endif

        public void ShowInterstitial()
        {
#if YandexGamesPlatform_yg
            Debug.Log($"[AdController] ShowInterstitial → таймер готов: {YG2.isTimerAdvCompleted} осталось: {YG2.timerInterAdv:F1}s");
            YGInsides.ResetTimerInterAdv();
            YG2.optionalPlatform.FirstInterAdvShow();
#else
            Debug.Log("[AdController] ShowInterstitial (stub — YG не установлен)");
#endif
        }

        public void ShowRewarded(string id, Action onReward)
        {
#if YandexGamesPlatform_yg
            YG2.RewardedAdvShow(id, onReward);
#else
            Debug.Log($"[AdController] ShowRewarded '{id}' (stub) — награда выдана");
            onReward?.Invoke();
#endif
        }
    }
}
