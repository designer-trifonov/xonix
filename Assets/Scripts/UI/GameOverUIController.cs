using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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


        private void Awake()
        {
            _panel.SetActive(false);
            _watchAdButton.onClick.AddListener(OnWatchAdClicked);
            _restartButton.onClick.AddListener(OnRestartClicked);
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
            Hide();
            OnWatchAd?.Invoke();
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
