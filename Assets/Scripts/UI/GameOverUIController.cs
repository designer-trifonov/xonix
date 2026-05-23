using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HippoGame.Interfaces;

namespace HippoGame.UI
{
    public class GameOverUIController : MonoBehaviour
    {
        [SerializeField] private GameObject  _panel;
        [SerializeField] private Button      _watchAdButton;
        [SerializeField] private Button      _restartButton;
        [SerializeField] private TMP_Text    _adButtonText;
        [SerializeField] private AdConfirmPopup _confirmPopup;

        private IPauseService _pause;

        public event Action OnWatchAd;
        public event Action OnRestart;

        public void Inject(IPauseService pause) => _pause = pause;

        private void Awake()
        {
            _panel.SetActive(false);
            _watchAdButton.onClick.AddListener(OnWatchAdClicked);
            _restartButton.onClick.AddListener(OnRestartClicked);
        }

        public void Show()
        {
            _panel.SetActive(true);
            _pause?.Pause();
        }

        public void Hide()
        {
            _panel.SetActive(false);
            _pause?.Resume();
        }

        private void OnWatchAdClicked()
        {
            if (_confirmPopup != null)
            {
                _confirmPopup.Show(() =>
                {
                    Hide();
                    OnWatchAd?.Invoke();
                });
            }
            else
            {
                Hide();
                OnWatchAd?.Invoke();
            }
        }

        private void OnRestartClicked()
        {
            Hide();
            OnRestart?.Invoke();
        }

        private void SetButtonsInteractable(bool value)
        {
            _watchAdButton.interactable = value;
            _restartButton.interactable = value;
        }
    }
}
