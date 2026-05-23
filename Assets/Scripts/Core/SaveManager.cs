using System.Collections.Generic;
using UnityEngine;
using YG;
using HippoGame.Interfaces;

namespace HippoGame.Core
{
    /// Единственное место работы с YG2.saves.
    /// Записывает, читает, очищает — и больше ничего.
    public class SaveManager : MonoBehaviour, ISaveManager
    {
        public bool HasSavedGame => YG2.saves.savedLevel > 0;

        // ── Запись ───────────────────────────────────────────────────────────────────

        public void Save(GameSnapshot snap)
        {
            var s = YG2.saves;
            s.savedScore       = snap.Score;
            s.savedLevel       = snap.Level;
            s.savedLives       = snap.Lives;
            s.savedDifficulty  = snap.Difficulty;
            s.savedLastFillPct = snap.LastFillPct;
            s.savedGridCells   = snap.GridCells;
            s.savedHippoCell   = $"{snap.HippoCell.x},{snap.HippoCell.y}";
            s.savedHippoDir    = $"{snap.HippoDir.x},{snap.HippoDir.y}";
            s.savedIsDrawing   = snap.IsDrawing ? 1 : 0;
            s.savedTrailPoints = SerializeTrail(snap.TrailPoints);
            s.savedBalls       = SerializeBalls(snap.Balls);
            s.savedBallSpeed   = snap.BallSpeed;

            YG2.SaveProgress(); // локально всегда, в облако только если авторизован
            Debug.Log($"[SaveManager] Saved: level={snap.Level} score={snap.Score} balls={snap.Balls.Count}");
        }

        // ── Чтение ───────────────────────────────────────────────────────────────────

        public GameSnapshot LoadSnapshot()
        {
            var s = YG2.saves;
            return new GameSnapshot
            {
                Score       = s.savedScore,
                Level       = s.savedLevel,
                Lives       = s.savedLives,
                Difficulty  = s.savedDifficulty,
                LastFillPct = s.savedLastFillPct,
                GridCells   = s.savedGridCells,
                HippoCell   = ParseVec2Int(s.savedHippoCell),
                HippoDir    = ParseVec2Int(s.savedHippoDir),
                IsDrawing   = s.savedIsDrawing == 1,
                TrailPoints = ParseTrail(s.savedTrailPoints),
                Balls       = ParseBalls(s.savedBalls),
                BallSpeed   = s.savedBallSpeed,
            };
        }

        // ── Сброс ────────────────────────────────────────────────────────────────────

        public void ClearSave()
        {
            var s = YG2.saves;
            s.savedScore       = 0;
            s.savedLevel       = 0;
            s.savedLives       = 0;
            s.savedDifficulty  = 1;
            s.savedLastFillPct = 0f;
            s.savedGridCells   = "";
            s.savedHippoCell   = "";
            s.savedHippoDir    = "";
            s.savedIsDrawing   = 0;
            s.savedTrailPoints = "";
            s.savedBalls       = "";
            s.savedBallSpeed   = 0f;

            YG2.SaveProgress();
            Debug.Log("[SaveManager] Save cleared");
        }

        // ── Сериализация ─────────────────────────────────────────────────────────────

        private static string SerializeTrail(List<Vector2Int> pts)
        {
            if (pts == null || pts.Count == 0) return "";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < pts.Count; i++)
            {
                if (i > 0) sb.Append(';');
                sb.Append(pts[i].x).Append(',').Append(pts[i].y);
            }
            return sb.ToString();
        }

        private static string SerializeBalls(List<BallSnapshot> balls)
        {
            if (balls == null || balls.Count == 0) return "";
            var ic = System.Globalization.CultureInfo.InvariantCulture;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < balls.Count; i++)
            {
                if (i > 0) sb.Append('|');
                sb.Append(balls[i].Position.x.ToString("F4", ic)).Append(',')
                  .Append(balls[i].Position.y.ToString("F4", ic)).Append(',')
                  .Append(balls[i].Direction.x.ToString("F4", ic)).Append(',')
                  .Append(balls[i].Direction.y.ToString("F4", ic));
            }
            return sb.ToString();
        }

        // ── Десериализация ───────────────────────────────────────────────────────────

        private static Vector2Int ParseVec2Int(string s)
        {
            if (string.IsNullOrEmpty(s)) return Vector2Int.zero;
            var p = s.Split(',');
            return p.Length == 2
                && int.TryParse(p[0], out int x)
                && int.TryParse(p[1], out int y)
                ? new Vector2Int(x, y) : Vector2Int.zero;
        }

        private static List<Vector2Int> ParseTrail(string s)
        {
            var list = new List<Vector2Int>();
            if (string.IsNullOrEmpty(s)) return list;
            foreach (var seg in s.Split(';'))
            {
                var p = seg.Split(',');
                if (p.Length == 2 && int.TryParse(p[0], out int x) && int.TryParse(p[1], out int y))
                    list.Add(new Vector2Int(x, y));
            }
            return list;
        }

        private static List<BallSnapshot> ParseBalls(string s)
        {
            var list = new List<BallSnapshot>();
            if (string.IsNullOrEmpty(s)) return list;
            var ic = System.Globalization.CultureInfo.InvariantCulture;
            foreach (var seg in s.Split('|'))
            {
                var p = seg.Split(',');
                if (p.Length == 4
                    && float.TryParse(p[0], System.Globalization.NumberStyles.Float, ic, out float px)
                    && float.TryParse(p[1], System.Globalization.NumberStyles.Float, ic, out float py)
                    && float.TryParse(p[2], System.Globalization.NumberStyles.Float, ic, out float dx)
                    && float.TryParse(p[3], System.Globalization.NumberStyles.Float, ic, out float dy))
                    list.Add(new BallSnapshot(new Vector2(px, py), new Vector2(dx, dy)));
            }
            return list;
        }
    }
}
