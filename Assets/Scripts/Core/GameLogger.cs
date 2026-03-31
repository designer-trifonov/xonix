using System;
using System.IO;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Core
{
    /// Пишет логи в файл Assets/Logs/session_*.log и дублирует в Debug.Log.
    public class GameLogger : IGameLogger
    {
        private string      _filePath;
        private bool        _initialized;
        private StreamWriter _writer;

        private void Init()
        {
            if (_initialized) return;
            _initialized = true;

            string logsDir = Path.Combine(Application.dataPath, "Logs");
            Directory.CreateDirectory(logsDir);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _filePath = Path.Combine(logsDir, $"session_{timestamp}.log");

            _writer = new StreamWriter(_filePath, append: false) { AutoFlush = true };
            _writer.WriteLine($"=== HippoGame session {timestamp} ===");

            Application.quitting += Flush;

            Debug.Log($"[GameLogger] Лог пишется в: {_filePath}");
        }

        public void Log(string msg)
        {
            Init();
            string line = $"[{Time.frameCount:D6}] {msg}";
            _writer?.WriteLine(line);
            Debug.Log(msg);
        }

        public void Warn(string msg)
        {
            Init();
            string line = $"[{Time.frameCount:D6}] WARN {msg}";
            _writer?.WriteLine(line);
            Debug.LogWarning(msg);
        }

        public void Error(string msg)
        {
            Init();
            string line = $"[{Time.frameCount:D6}] ERROR {msg}";
            _writer?.WriteLine(line);
            Debug.LogError(msg);
        }

        public void Flush()
        {
            _writer?.Flush();
            _writer?.Close();
            _writer = null;
        }
    }
}
