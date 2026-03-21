using UnityEngine;
using HippoGame.Grid;
using HippoGame.Interfaces;

namespace HippoGame.Movement
{
    /// Умная коллизия для гиппо:
    /// — рисует трейл → проходит сквозь залитые зоны свободно
    /// — стоит на краю → тоже пропускаем (старт рисования)
    /// — залил, в поле → CellCollisionService блокирует по клеткам
    public class DrawingAwareCollisionService : ICollisionService
    {
        private readonly ICollisionService _cellCollision;
        private readonly IDrawingState     _drawingState;
        private readonly IGridService      _grid;

        public DrawingAwareCollisionService(IDrawingState drawingState, IGridService grid, ICollisionService cellCollision)
        {
            _cellCollision = cellCollision;
            _drawingState  = drawingState;
            _grid          = grid;
            Debug.Log("[DrawingAwareCollisionService] Создан");
        }

        public Vector2 ClampToFree(Vector2 from, Vector2 to)
        {
            if (_drawingState.IsDrawing)
                return to;

            if (_grid.IsEdge(_grid.WorldToCell(from)))
                return to;

            // Не рисуем, не на edge: разрешаем движение в Filled (своя территория)
            Vector2Int toCell = _grid.WorldToCell(to);
            if (_grid.IsInBounds(toCell) && _grid.GetCell(toCell.x, toCell.y) == CellState.Filled)
                return to;

            return _cellCollision.ClampToFree(from, to);
        }
    }
}
