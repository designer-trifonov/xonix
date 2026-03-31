using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Hippo
{
    /// Записывает клетки трейла в grid и очищает их.
    /// Не знает о состоянии рисования, событиях или визуале.
    public class TrailWriter
    {
        private readonly IGridService  _grid;
        private readonly ITrailService _trail;

        public TrailWriter(IGridService grid, ITrailService trail)
        {
            _grid  = grid;
            _trail = trail;
        }

        public void CommitSegment(Vector2Int from, Vector2Int to)
        {
            if (from == to) return;

            Vector2Int diff = to - from;
            Vector2Int step;
            int        steps;

            bool diagonal = diff.x != 0 && diff.y != 0;
            if (diagonal)
            {
                steps = Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
                step  = new Vector2Int(diff.x > 0 ? 1 : -1, diff.y > 0 ? 1 : -1);
            }
            else if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
            {
                steps = Mathf.Abs(diff.x);
                step  = new Vector2Int(diff.x > 0 ? 1 : -1, 0);
            }
            else
            {
                steps = Mathf.Abs(diff.y);
                step  = new Vector2Int(0, diff.y > 0 ? 1 : -1);
            }

            for (int i = 0; i <= steps; i++)
            {
                Vector2Int c  = from + step * i;
                if (!_grid.IsInBounds(c)) break;
                CellState  cs = _grid.GetCell(c.x, c.y);
                if (cs == CellState.Empty)
                {
                    _trail.AddPoint(c);
                    _grid.SetCell(c.x, c.y, CellState.Trail);
                }
                else if (cs == CellState.Filled)
                {
                    _trail.AddPoint(c); // визуал — не меняем grid
                }
            }
        }

        public void ClearTrailCells(IReadOnlyList<Vector2Int> points)
        {
            foreach (Vector2Int c in points)
                if (_grid.GetCell(c.x, c.y) == CellState.Trail)
                    _grid.SetCell(c.x, c.y, CellState.Empty);
        }
    }
}
