using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YG;

namespace HippoGame.UI
{
    public class UILocalizer : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public TMP_Text target;
            [TextArea(1, 5)] public string ru;
            [TextArea(1, 5)] public string en;
        }

        [SerializeField] private List<Entry> _entries;

        private void OnEnable()
        {
            Apply();
            YG2.onSwitchLang += OnLangChanged;
        }

        private void OnDisable() => YG2.onSwitchLang -= OnLangChanged;

        private void OnLangChanged(string _) => Apply();

        private void Apply()
        {
            foreach (var e in _entries)
            {
                if (e.target == null) continue;
                e.target.text = YG2.lang == "en" ? e.en : e.ru;
            }
        }
    }
}
