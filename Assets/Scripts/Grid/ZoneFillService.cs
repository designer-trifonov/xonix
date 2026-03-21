using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Grid
{
    /// Заливает зону пикселями через FloodFillService. Без геометрии.
    public class ZoneFillService : IFillService
    {
        private readonly FloodFillService _flood = new();

        public ZoneFillService()
        {
            Debug.Log("[ZoneFillService] Создан");
        }

        public void Fill(IGridService grid, List<Vector2Int> trail)
        {
            Debug.Log($"[ZoneFillService] Fill | trail={trail.Count} пикселей");
            _flood.Fill(grid, trail);
        }
    }
}
