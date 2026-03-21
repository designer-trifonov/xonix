using System;
using System.Collections;
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
            StartCoroutine(MockAd());
        }

        private IEnumerator MockAd()
        {
            _watchAdButton.interactable = false;
            _restartButton.interactable = false;

            // Мок рекламы — считаем 5 секунд
            for (int i = 5; i > 0; i--)
            {
                if (_adButtonText != null)
                    _adButtonText.text = $"Реклама... {i}";
                yield return new WaitForSecondsRealtime(1f);
            }

            if (_adButtonText != null)
                _adButtonText.text = "Продолжить за рекламу";

            _watchAdButton.interactable = true;
            _restartButton.interactable = true;

            Hide();
            OnWatchAd?.Invoke();
        }

        private void OnRestartClicked()
        {
            Hide();
            OnRestart?.Invoke();
        }
    }
}
