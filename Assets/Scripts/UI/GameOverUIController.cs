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
            Debug.Log("[GameOverUIController] Show — игра на паузе");
        }

        public void Hide()
        {
            _panel.SetActive(false);
            Time.timeScale = 1f;
            Debug.Log("[GameOverUIController] Hide — игра возобновлена");
        }

        private void OnWatchAdClicked()
        {
            Debug.Log("[GameOverUIController] OnWatchAdClicked");
            StartCoroutine(MockAd());
        }

        private IEnumerator MockAd()
        {
            Debug.Log("[GameOverUIController] MockAd — старт");
            _watchAdButton.interactable = false;
            _restartButton.interactable = false;

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

            Debug.Log("[GameOverUIController] MockAd — завершена, продолжаем");
            Hide();
            OnWatchAd?.Invoke();
        }

        private void OnRestartClicked()
        {
            Debug.Log("[GameOverUIController] OnRestartClicked");
            Hide();
            OnRestart?.Invoke();
        }
    }
}
