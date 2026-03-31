using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Core
{
    public class ScreenLogger : MonoBehaviour
    {
        [SerializeField] private int   _maxLines    = 20;
        [SerializeField] private int   _fontSize    = 22;
        [SerializeField] private float _bgAlpha     = 0.55f;
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;

        private readonly List<(string text, LogType type)> _lines = new();
        private bool    _visible = true;
        private GUIStyle _style;
        private Texture2D _bg;

        private void OnEnable()  => Application.logMessageReceived += OnLog;
        private void OnDisable() => Application.logMessageReceived -= OnLog;

        private void OnLog(string message, string stackTrace, LogType type)
        {
            _lines.Add((message, type));
            if (_lines.Count > _maxLines)
                _lines.RemoveAt(0);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(_toggleKey))
                _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible) return;

            if (_style == null)
            {
                _bg = new Texture2D(1, 1);
                _bg.SetPixel(0, 0, new Color(0f, 0f, 0f, _bgAlpha));
                _bg.Apply();

                _style = new GUIStyle(GUI.skin.label)
                {
                    fontSize  = _fontSize,
                    wordWrap  = true,
                    richText  = true,
                    padding   = new RectOffset(8, 8, 4, 4)
                };
                _style.normal.background = _bg;
            }

            float w = Screen.width * 0.55f;
            float lineH = _fontSize + 6f;
            float h = _lines.Count * lineH + 8f;
            float y = Screen.height - h - 4f;

            GUI.BeginGroup(new Rect(4, y, w, h));
            for (int i = 0; i < _lines.Count; i++)
            {
                var (text, type) = _lines[i];
                _style.normal.textColor = type switch
                {
                    LogType.Error     => Color.red,
                    LogType.Exception => Color.red,
                    LogType.Warning   => Color.yellow,
                    _                 => Color.white
                };
                GUI.Label(new Rect(0, i * lineH, w, lineH), text, _style);
            }
            GUI.EndGroup();
        }
    }
}
