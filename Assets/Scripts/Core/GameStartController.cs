using System;
using System.Collections.Generic;
using UnityEngine;
using HippoGame.UI;
using HippoGame.Interfaces;

namespace HippoGame.Core
{
    public class GameStartController : MonoBehaviour
    {
        [SerializeField] private RulesPopupController _rulesPopup;
        [SerializeField] private GameObject[]         _hideUntilStart;

        private readonly List<Action> _initCallbacks = new();
        private IAdService _adService;

        public void Inject(IAdService adService) => _adService = adService;

        public void RegisterInit(Action callback) => _initCallbacks.Add(callback);

        private void Start()
        {
            SetVisible(false);

            if (_rulesPopup != null)
            {
                _rulesPopup.OnClose += OnGameStart;
                _rulesPopup.Show();
            }
            else
            {
                OnGameStart();
            }
        }

        private void OnGameStart()
        {
            _adService?.ShowInterstitial();
            SetVisible(true);
            foreach (var cb in _initCallbacks)
                cb?.Invoke();
            Debug.Log("[GameStartController] Игра запущена");
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }

        private void SetVisible(bool visible)
        {
            if (_hideUntilStart == null) return;
            foreach (var go in _hideUntilStart)
                if (go != null) go.SetActive(visible);
        }
    }
}
