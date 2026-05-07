using UnityEngine;
using YG;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Core
{
    public class GameSessionSaver : MonoBehaviour
    {
        private IGameState   _gameState;
        private IGridService _grid;
        private LevelManager _levelManager;
        private bool         _active;

        public bool HasSavedGame => YG2.saves.savedLevel > 0;

        public void Inject(IGameState gameState, IGridService grid, LevelManager levelManager)
        {
            _gameState    = gameState;
            _grid         = grid;
            _levelManager = levelManager;
        }

        private void OnEnable()
        {
            YG2.onPauseGame       += OnPauseGame;
            YG2.onFocusWindowGame += OnFocusWindow;
        }

        private void OnDisable()
        {
            YG2.onPauseGame       -= OnPauseGame;
            YG2.onFocusWindowGame -= OnFocusWindow;
        }

        // Вызывается после завершения инициализации уровня
        public void Activate()
        {
            if (_active) return;
            _active = true;
            _gameState.OnChanged        += SaveGame;
            _levelManager.OnLevelStarted += SaveGame;
            Debug.Log("[GameSessionSaver] Активирован");
        }

        public void Deactivate()
        {
            if (!_active) return;
            _active = false;
            _gameState.OnChanged        -= SaveGame;
            _levelManager.OnLevelStarted -= SaveGame;
            Debug.Log("[GameSessionSaver] Деактивирован");
        }

        public void SaveGame()
        {
            if (!_active) return;
            YG2.saves.savedScore       = _gameState.Score;
            YG2.saves.savedLevel       = _gameState.Level;
            YG2.saves.savedLives       = _gameState.Lives;
            YG2.saves.savedDifficulty  = (int)_gameState.Difficulty;
            YG2.saves.savedLastFillPct = _levelManager.LastFillPct;
            YG2.saves.savedGridCells   = SerializeGrid();
            YG2.SaveProgress();
            Debug.Log($"[GameSessionSaver] Сохранено: score={_gameState.Score} level={_gameState.Level} lives={_gameState.Lives}");
        }

        public void ClearSave()
        {
            Deactivate();
            YG2.saves.savedLevel     = 0;
            YG2.saves.savedGridCells = "";
            YG2.SaveProgress();
            Debug.Log("[GameSessionSaver] Сохранение сброшено");
        }

        // Восстанавливает Score/Level/Lives/Difficulty в GameState до запуска init-коллбэков
        public void RestoreToGameState()
        {
            _gameState.RestoreSession(
                YG2.saves.savedScore,
                YG2.saves.savedLevel,
                YG2.saves.savedLives,
                (Difficulty)YG2.saves.savedDifficulty);
        }

        // Восстанавливает ячейки поля и lastFillPct — вызывать ПОСЛЕ init-коллбэков
        public void RestoreGridAndFill()
        {
            DeserializeGrid(YG2.saves.savedGridCells);
            _levelManager.RestoreLastFillPct(YG2.saves.savedLastFillPct);
            Debug.Log($"[GameSessionSaver] Поле восстановлено, fillPct={YG2.saves.savedLastFillPct:F1}%");
        }

        private void OnPauseGame(bool isPause)
        {
            if (isPause) SaveGame();
        }

        private void OnFocusWindow(bool hasFocus)
        {
            if (!hasFocus) SaveGame();
        }

        private string SerializeGrid()
        {
            int    cols = _grid.Columns;
            int    rows = _grid.Rows;
            byte[] data = new byte[cols * rows];
            for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
                data[x * rows + y] = (byte)_grid.GetCell(x, y);
            return System.Convert.ToBase64String(data);
        }

        private void DeserializeGrid(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return;
            byte[] data = System.Convert.FromBase64String(base64);
            int    cols = _grid.Columns;
            int    rows = _grid.Rows;
            for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
            {
                int idx = x * rows + y;
                if (idx < data.Length)
                    _grid.SetCell(x, y, (CellState)data[idx]);
            }
            _grid.RefreshRenderer();
        }
    }
}
