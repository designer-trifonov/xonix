using UnityEngine;
using HippoGame.Grid;

namespace HippoGame.Interfaces
{
    public interface IGridService
    {
        int   Columns  { get; }
        int   Rows     { get; }
        Rect  Bounds   { get; }
        float CellSize { get; }
        CellState GetCell(int x, int y);
        void SetCell(int x, int y, CellState state);
        Vector2Int WorldToCell(Vector2 worldPos);
        Vector2    CellToWorld(Vector2Int cell);
        bool IsInBounds(Vector2Int cell);
        bool IsEdge(Vector2Int cell);
        void ResetCells();
        void RefreshRenderer();
    }
}
