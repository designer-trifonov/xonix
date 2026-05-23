using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Core
{
    /// Снимок полного игрового состояния — только данные, никакой логики.
    public class GameSnapshot
    {
        // Игровое состояние
        public int   Score;
        public int   Level;        // 0 = нет сохранения
        public int   Lives;
        public int   Difficulty;
        public float LastFillPct;

        // Поле (base64-кодированные CellState)
        public string GridCells = "";

        // Гиппо
        public Vector2Int HippoCell;
        public Vector2Int HippoDir;
        public bool       IsDrawing;

        // Трейл (упорядоченный список клеток)
        public List<Vector2Int> TrailPoints = new();

        // Арбузы
        public List<BallSnapshot> Balls     = new();
        public float              BallSpeed;
    }

    public readonly struct BallSnapshot
    {
        public readonly Vector2 Position;
        public readonly Vector2 Direction;

        public BallSnapshot(Vector2 position, Vector2 direction)
        {
            Position  = position;
            Direction = direction;
        }
    }
}
