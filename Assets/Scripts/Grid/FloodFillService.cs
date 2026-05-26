using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Grid
{
    /// Flood fill 4-connected: находит регионы Empty клеток, заливает те, в которых нет шара.
    /// Шар проверяется по ячейке + все 8 соседей (на случай если шар на Border/Trail/Filled).
    public class FloodFillService : IFillService
    {
        private static readonly Vector2Int[] Cardinals =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        private static readonly Vector2Int[] AllNeighbors =
        {
            new Vector2Int( 0,  1), new Vector2Int( 0, -1),
            new Vector2Int( 1,  0), new Vector2Int(-1,  0),
            new Vector2Int( 1,  1), new Vector2Int( 1, -1),
            new Vector2Int(-1,  1), new Vector2Int(-1, -1)
        };

        public void Fill(IGridService grid, List<Vector2Int> trail, IReadOnlyList<Vector2> ballPositions)
        {
            // 1. Находим все регионы Empty клеток
            var visited = new HashSet<Vector2Int>();
            var regions = new List<List<Vector2Int>>();

            for (int x = 0; x < grid.Columns; x++)
            for (int y = 0; y < grid.Rows; y++)
            {
                var cell = new Vector2Int(x, y);
                if (visited.Contains(cell))                continue;
                if (grid.GetCell(x, y) != CellState.Empty) continue;

                regions.Add(FloodRegion(grid, visited, cell));
            }

            // 2. Собираем ячейки шаров — включая соседей, чтобы поймать шар на Border/Trail/Filled
            var ballEmptyCells = new HashSet<Vector2Int>();
            if (ballPositions != null)
            {
                foreach (var bp in ballPositions)
                {
                    var bc = grid.WorldToCell(bp);
                    // Если шар на Empty — берём как есть
                    if (grid.IsInBounds(bc) && grid.GetCell(bc.x, bc.y) == CellState.Empty)
                    {
                        ballEmptyCells.Add(bc);
                    }
                    else
                    {
                        // Шар на Border/Trail/Filled — берём ВСЕ пустые соседние ячейки,
                        // чтобы защитить все смежные регионы, а не только первый найденный.
                        foreach (var dir in AllNeighbors)
                        {
                            var n = bc + dir;
                            if (grid.IsInBounds(n) && grid.GetCell(n.x, n.y) == CellState.Empty)
                                ballEmptyCells.Add(n);
                        }
                    }
                }
            }

            Debug.Log($"[FloodFillService] шаров={ballPositions?.Count ?? 0} ballEmptyCells={ballEmptyCells.Count}");

            int filled = 0;
            foreach (var region in regions)
            {
                bool hasBall = false;
                foreach (var c in region)
                    if (ballEmptyCells.Contains(c)) { hasBall = true; break; }

                Debug.Log($"[FloodFillService] регион size={region.Count} hasBall={hasBall}");

                if (!hasBall)
                {
                    foreach (var c in region)
                        grid.SetCell(c.x, c.y, CellState.Filled);
                    filled++;
                }
            }

            // 3. Trail → Filled
            for (int x = 0; x < grid.Columns; x++)
            for (int y = 0; y < grid.Rows; y++)
                if (grid.GetCell(x, y) == CellState.Trail)
                    grid.SetCell(x, y, CellState.Filled);

            Debug.Log($"[FloodFillService] регионов={regions.Count} залито={filled}");
        }

        private static List<Vector2Int> FloodRegion(IGridService grid,
            HashSet<Vector2Int> visited, Vector2Int start)
        {
            var region = new List<Vector2Int>();
            var queue  = new Queue<Vector2Int>();
            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                region.Add(cell);

                foreach (var dir in Cardinals)
                {
                    var n = cell + dir;
                    if (!grid.IsInBounds(n))                       continue;
                    if (visited.Contains(n))                        continue;
                    if (grid.GetCell(n.x, n.y) != CellState.Empty) continue;

                    visited.Add(n);
                    queue.Enqueue(n);
                }
            }

            return region;
        }
    }
}
