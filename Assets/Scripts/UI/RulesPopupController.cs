using System;
using UnityEngine;
using UnityEngine.UI;

namespace HippoGame.UI
{
    public class RulesPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button     _closeButton;

        public event Action OnClose;

        public void Initialize()
        {
            Debug.Log("[RulesPopupController] Initialize");
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        public void Show()
        {
            _panel.SetActive(true);
        }

        private void OnCloseClicked()
        {
            _panel.SetActive(false);
            OnClose?.Invoke();
        }
    }
}
