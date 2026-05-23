using System;
using UnityEngine;
using UnityEngine.UI;

namespace HippoGame.UI
{
    public class MainMenuButtonController : MonoBehaviour
    {
        [SerializeField] private Button _button;

        public event Action OnMainMenu;

        private void Awake()
        {
            _button.onClick.AddListener(() => OnMainMenu?.Invoke());
        }
    }
}
