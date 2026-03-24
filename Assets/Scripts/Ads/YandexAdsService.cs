using System;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace HippoGame.Ads
{
    /// Единая точка работы с Яндекс рекламой.
    /// Баннер — показывается постоянно. Rewarded — для бустов и продолжения игры.
    public class YandexAdsService : MonoBehaviour
    {
        // Тестовые ID. Заменить на реальные R-M-XXXXXX-Y перед релизом.
        private const string BannerId     = "demo-banner-yandex";
        private const string RewardedId   = "demo-rewarded-yandex";

        private Banner  _banner;
        private RewardedAd _rewarded;

        private Action _onRewardedSuccess;
        private Action _onRewardedFailed;

        private bool _rewardGranted;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ShowBanner();
            LoadRewarded();
        }

        // ── Баннер ───────────────────────────────────────────────────────────────

        private void ShowBanner()
        {
            _banner = new Banner(BannerId, BannerAdSize.StickySize(320), AdPosition.BottomCenter);
            _banner.OnAdLoaded    += (s, e) => { Debug.Log("[Ads] Баннер загружен"); _banner.Show(); };
            _banner.OnAdFailedToLoad += (s, e) => Debug.LogWarning($"[Ads] Баннер не загружен: {e.Message}");
            _banner.LoadAd(new AdRequest.Builder().Build());
        }

        // ── Rewarded ─────────────────────────────────────────────────────────────

        private void LoadRewarded()
        {
            var loader = new RewardedAdLoader();
            loader.OnAdLoaded        += (s, e) => { _rewarded = e.RewardedAd; SubscribeRewarded(); Debug.Log("[Ads] Rewarded загружен"); };
            loader.OnAdFailedToLoad  += (s, e) => Debug.LogWarning($"[Ads] Rewarded не загружен: {e.Message}");
            loader.LoadAd(new AdRequestConfiguration.Builder(RewardedId).Build());
        }

        private void SubscribeRewarded()
        {
            _rewarded.OnRewarded       += (s, e) => { _rewardGranted = true; Debug.Log("[Ads] Rewarded — награда"); };
            _rewarded.OnAdDismissed    += (s, e) => HandleRewardedClosed();
            _rewarded.OnAdFailedToShow += (s, e) => { Debug.LogWarning($"[Ads] Rewarded не показан: {e.Message}"); HandleRewardedClosed(failed: true); };
        }

        private void HandleRewardedClosed(bool failed = false)
        {
            if (!failed && _rewardGranted)
                _onRewardedSuccess?.Invoke();
            else
                _onRewardedFailed?.Invoke();

            _rewardGranted      = false;
            _onRewardedSuccess  = null;
            _onRewardedFailed   = null;

            _rewarded = null;
            LoadRewarded(); // предзагружаем следующий
        }

        /// Показать rewarded рекламу.
        /// onSuccess — вызывается если досмотрел до конца.
        /// onFailed  — вызывается если закрыл раньше или ошибка.
        public void ShowRewarded(Action onSuccess, Action onFailed = null)
        {
            if (_rewarded == null)
            {
                Debug.LogWarning("[Ads] Rewarded не готов, пробуем загрузить снова");
                onFailed?.Invoke();
                LoadRewarded();
                return;
            }

            _onRewardedSuccess = onSuccess;
            _onRewardedFailed  = onFailed;
            _rewardGranted     = false;
            _rewarded.Show();
        }
    }
}
