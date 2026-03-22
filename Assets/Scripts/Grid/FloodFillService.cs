using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Grid
{
    /// Flood fill: находит замкнутые регионы (без выхода к краю), заливает их.
    public class FloodFillService : IFillService
    {
        private static readonly Vector2Int[] Neighbours =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        public void Fill(IGridService grid, List<Vector2Int> trail, IReadOnlyList<Vector2> ballPositions)
        {
            Debug.Log($"[FloodFillService] ── АНАЛИЗ ЗОНЫ ── trail={trail.Count} кл");

            HashSet<Vector2Int>    visited = new();
            List<List<Vector2Int>> regions = new();

            for (int x = 0; x < grid.Columns; x++)
            for (int y = 0; y < grid.Rows; y++)
            {
                Vector2Int cell = new(x, y);
                if (visited.Contains(cell))                continue;
                if (grid.GetCell(x, y) != CellState.Empty) continue;

                regions.Add(FloodRegion(grid, visited, cell));
            }

            Debug.Log($"[FloodFillService] Найдено регионов: {regions.Count}");

            if (regions.Count == 0)
            {
                Debug.Log("[FloodFillService] Регионов нет — ничего не заливаем");
                return;
            }

            // Наибольший регион = внешний (соединён с основным полем).
            // Все остальные — внутренние, заливаем (если нет шара).
            int maxIdx = 0;
            for (int i = 1; i < regions.Count; i++)
                if (regions[i].Count > regions[maxIdx].Count)
                    maxIdx = i;

            int filled = 0;
            for (int i = 0; i < regions.Count; i++)
            {
                if (i == maxIdx)
                {
                    Debug.Log($"[FloodFillService]   Регион {i}: {regions[i].Count} кл — ВНЕШНИЙ (наибольший), пропуск");
                    continue;
                }

                var region    = regions[i];
                var regionSet = new HashSet<Vector2Int>(region);
                bool hasBall  = false;
                Vector2 ballPos = default;

                if (ballPositions != null)
                    foreach (Vector2 bp in ballPositions)
                    {
                        Vector2Int bc = grid.WorldToCell(bp);
                        if (regionSet.Contains(bc)) { hasBall = true; ballPos = bp; break; }
                    }

                if (hasBall)
                    Debug.Log($"[FloodFillService]   Регион {i}: {region.Count} кл — ШАР внутри pos={ballPos}, пропуск");
                else
                {
                    foreach (var c in region)
                        grid.SetCell(c.x, c.y, CellState.Filled);
                    Debug.Log($"[FloodFillService]   Регион {i}: {region.Count} кл — ЗАЛИТ");
                    filled++;
                }
            }

            // Trail → Filled
            int trailCells = 0;
            for (int x = 0; x < grid.Columns; x++)
            for (int y = 0; y < grid.Rows; y++)
                if (grid.GetCell(x, y) == CellState.Trail)
                {
                    grid.SetCell(x, y, CellState.Filled);
                    trailCells++;
                }

            // Итог: всего закрашено
            int totalFilled = 0;
            int total       = grid.Columns * grid.Rows;
            for (int x = 0; x < grid.Columns; x++)
            for (int y = 0; y < grid.Rows; y++)
                if (grid.GetCell(x, y) == CellState.Filled) totalFilled++;

            float pct = total > 0 ? totalFilled * 100f / total : 0f;
            Debug.Log($"[FloodFillService] ══ ИТОГ ══ залито регионов={filled} трейл={trailCells} кл | всего закрашено={totalFilled}/{total} ({pct:F1}%)");
        }

        private static List<Vector2Int> FloodRegion(IGridService grid,
            HashSet<Vector2Int> visited, Vector2Int start)
        {
            List<Vector2Int>  region = new();
            Queue<Vector2Int> queue  = new();
            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                region.Add(cell);

                foreach (Vector2Int dir in Neighbours)
                {
                    Vector2Int n = cell + dir;
                    if (!grid.IsInBounds(n))                        continue;
                    if (visited.Contains(n))                         continue;
                    if (grid.GetCell(n.x, n.y) != CellState.Empty)  continue;

                    visited.Add(n);
                    queue.Enqueue(n);
                }
            }

            return region;
        }
    }
}
