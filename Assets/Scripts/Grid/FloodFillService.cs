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

            // Наименьший регион без шара = внутренний, заливаем его
            int minIdx = -1;
            for (int i = 0; i < regions.Count; i++)
            {
                if (minIdx == -1 || regions[i].Count < regions[minIdx].Count)
                    minIdx = i;
            }

            int filled = 0;
            if (minIdx == -1)
            {
                Debug.Log("[FloodFillService] БРЕШЬ: регионов нет");
            }
            else
            {
                var region    = regions[minIdx];
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
                    Debug.Log($"[FloodFillService]   ШАР внутри наименьшего ({region.Count} кл) pos={ballPos} — не закрашиваем");
                else
                {
                    foreach (var c in region)
                        grid.SetCell(c.x, c.y, CellState.Filled);
                    Debug.Log($"[FloodFillService]   ОК: закрашен наименьший регион ({region.Count} кл)");
                    filled++;
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
