using System;

namespace HippoGame.Interfaces
{
    public interface IAdService
    {
        bool IsAdShowing { get; }

        event Action OnInterstitialClosed;
        event Action OnRewardedOpen;
        event Action OnRewardedClose;
        event Action OnRewardedError;

        void ShowInterstitial();
        void ShowRewarded(string id, Action onReward);
    }
}
