using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Movement
{
    /// Блокирует вход гиппо в Filled-клетки.
    /// При попытке войти — пробует скользить по одной оси.
    public class CellCollisionService : ICollisionService
    {
        private readonly IGridService _grid;

        public CellCollisionService(IGridService grid)
        {
            _grid = grid;
            Debug.Log("[CellCollisionService] Создан");
        }

        public Vector2 ClampToFree(Vector2 from, Vector2 to)
        {
            Vector2Int toCell = _grid.WorldToCell(to);

            if (!_grid.IsInBounds(toCell))
                return from;

            if (_grid.GetCell(toCell.x, toCell.y) != CellState.Filled)
                return to;

            // Пробуем скользить по горизонтали
            Vector2    slideX     = new Vector2(to.x, from.y);
            Vector2Int slideXCell = _grid.WorldToCell(slideX);
            if (_grid.IsInBounds(slideXCell) && _grid.GetCell(slideXCell.x, slideXCell.y) != CellState.Filled)
                return slideX;

            // Пробуем скользить по вертикали
            Vector2    slideY     = new Vector2(from.x, to.y);
            Vector2Int slideYCell = _grid.WorldToCell(slideY);
            if (_grid.IsInBounds(slideYCell) && _grid.GetCell(slideYCell.x, slideYCell.y) != CellState.Filled)
                return slideY;

            return from;
        }
    }
}
