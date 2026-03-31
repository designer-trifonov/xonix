using System;
using UnityEngine;
using HippoGame.Interfaces;

#if YandexGamesPlatform_yg
using YG;
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
            YG2.onGetSDKData       += OnSDKReady;
            YG2.onOpenRewardedAdv  += FireOpen;
            YG2.onCloseRewardedAdv += FireClose;
            YG2.onErrorRewardedAdv += FireError;
#endif
        }

        private void OnDisable()
        {
#if YandexGamesPlatform_yg
            YG2.onGetSDKData       -= OnSDKReady;
            YG2.onOpenRewardedAdv  -= FireOpen;
            YG2.onCloseRewardedAdv -= FireClose;
            YG2.onErrorRewardedAdv -= FireError;
#endif
        }

        private void FireOpen()  => OnRewardedOpen?.Invoke();
        private void FireClose() => OnRewardedClose?.Invoke();
        private void FireError() => OnRewardedError?.Invoke();

#if YandexGamesPlatform_yg
        private void OnSDKReady() => YG2.InterstitialAdvShow();
#endif

        public void ShowInterstitial()
        {
#if YandexGamesPlatform_yg
            YG2.InterstitialAdvShow();
#else
            Debug.Log("[AdController] ShowInterstitial — YG не установлен");
#endif
        }

        public void ShowRewarded(string id, Action onReward)
        {
#if YandexGamesPlatform_yg
            YG2.RewardedAdvShow(id, onReward);
#else
            Debug.Log($"[AdController] ShowRewarded '{id}' — YG не установлен, награда выдана");
            onReward?.Invoke();
#endif
        }
    }
}
