using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HippoGame.Ads;

namespace HippoGame.UI
{
    public class GameOverUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button     _watchAdButton;
        [SerializeField] private Button     _restartButton;
        [SerializeField] private TMP_Text   _adButtonText;

        public event Action OnWatchAd;
        public event Action OnRestart;

        private YandexAdsService _ads;

        private void Awake()
        {
            _panel.SetActive(false);
            _watchAdButton.onClick.AddListener(OnWatchAdClicked);
            _restartButton.onClick.AddListener(OnRestartClicked);
            _ads = FindObjectOfType<YandexAdsService>();
        }

        public void Show()
        {
            _panel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Hide()
        {
            _panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OnWatchAdClicked()
        {
            _watchAdButton.interactable = false;
            _restartButton.interactable = false;
            if (_adButtonText != null) _adButtonText.text = "Загрузка...";

            _ads.ShowRewarded(
                onSuccess: () =>
                {
                    if (_adButtonText != null) _adButtonText.text = "Продолжить за рекламу";
                    _watchAdButton.interactable = true;
                    _restartButton.interactable = true;
                    Hide();
                    OnWatchAd?.Invoke();
                },
                onFailed: () =>
                {
                    if (_adButtonText != null) _adButtonText.text = "Продолжить за рекламу";
                    _watchAdButton.interactable = true;
                    _restartButton.interactable = true;
                    Debug.Log("[GameOverUIController] Реклама не досмотрена");
                }
            );
        }

        private void OnRestartClicked()
        {
            Hide();
            OnRestart?.Invoke();
        }
    }
}
