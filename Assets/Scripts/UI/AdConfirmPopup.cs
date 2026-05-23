using System;
using UnityEngine;
using UnityEngine.UI;

namespace HippoGame.UI
{
    public class AdConfirmPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button     _confirmButton;
        [SerializeField] private Button     _cancelButton;

        private Action _onConfirm;
        private Action _onCancel;

        private void Start()
        {
            if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirm);
            else Debug.LogError("[AdConfirmPopup] _confirmButton не назначен!");

            if (_cancelButton != null) _cancelButton.onClick.AddListener(OnCancel);
            else Debug.LogError("[AdConfirmPopup] _cancelButton не назначен!");
        }

        /// <summary>onCancel вызывается если нажали Нет — используется чтобы вернуть предыдущую панель</summary>
        public void Show(Action onConfirm, Action onCancel = null)
        {
            _onConfirm = onConfirm;
            _onCancel  = onCancel;
            _panel.SetActive(true);
        }

        private void OnConfirm()
        {
            _panel.SetActive(false);
            var cb = _onConfirm;
            _onConfirm = null;
            _onCancel  = null;
            cb?.Invoke();
        }

        private void OnCancel()
        {
            _panel.SetActive(false);
            var cb = _onCancel;
            _onConfirm = null;
            _onCancel  = null;
            cb?.Invoke();
        }
    }
}
