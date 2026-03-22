using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Grid
{
    /// Flood fill: находит все пустые регионы, заливает наименьший.
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

            if (regions.Count == 0)
            {
                Debug.Log("[FloodFillService] Нет пустых регионов — нечего заливать");
            }
            else if (regions.Count == 1)
            {
                Debug.Log($"[FloodFillService] БРЕШЬ: найден только 1 регион ({regions[0].Count} кл) — трейл не замкнул зону");
            }
            else
            {
                Debug.Log($"[FloodFillService] Найдено регионов: {regions.Count}");
                for (int i = 0; i < regions.Count; i++)
                    Debug.Log($"[FloodFillService]   регион[{i}] = {regions[i].Count} кл");

                int minIdx = 0;
                for (int i = 1; i < regions.Count; i++)
                    if (regions[i].Count < regions[minIdx].Count)
                        minIdx = i;

                var minRegion    = regions[minIdx];
                var minRegionSet = new HashSet<Vector2Int>(minRegion);
                bool hasBall     = false;
                Vector2 ballCell = default;

                if (ballPositions != null)
                    foreach (Vector2 bp in ballPositions)
                    {
                        Vector2Int bc = grid.WorldToCell(bp);
                        if (minRegionSet.Contains(bc)) { hasBall = true; ballCell = bp; break; }
                    }

                if (hasBall)
                    Debug.Log($"[FloodFillService] ШАР в наименьшем регионе [{minIdx}] ({minRegion.Count} кл) pos={ballCell} — не закрашиваем");
                else
                {
                    foreach (Vector2Int c in minRegion)
                        grid.SetCell(c.x, c.y, CellState.Filled);
                    Debug.Log($"[FloodFillService] ОК: закрашен регион [{minIdx}] ({minRegion.Count} кл)");
                }
            }

            // Trail → Filled
            for (int x = 0; x < grid.Columns; x++)
            for (int y = 0; y < grid.Rows; y++)
                if (grid.GetCell(x, y) == CellState.Trail)
                    grid.SetCell(x, y, CellState.Filled);
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
