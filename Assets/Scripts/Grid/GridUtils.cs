using HippoGame.Interfaces;

namespace HippoGame.Grid
{
    public static class GridUtils
    {
        public static float CountFillPercent(IGridService grid)
        {
            int total = grid.Columns * grid.Rows;
            if (total == 0) return 0f;

            int filled = 0;
            for (int x = 0; x < grid.Columns; x++)
            for (int y = 0; y < grid.Rows; y++)
                if (grid.GetCell(x, y) == CellState.Filled)
                    filled++;

            return (float)filled / total * 100f;
        }
    }
}
