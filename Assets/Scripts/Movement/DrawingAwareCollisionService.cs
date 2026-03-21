using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Hippo;

namespace HippoGame.Movement
{
    /// Умная коллизия для гиппо:
    /// — рисует трейл → проходит сквозь залитые зоны свободно
    /// — стоит на краю → тоже пропускаем (старт рисования)
    /// — залил, в поле → CellCollisionService блокирует по клеткам
    public class DrawingAwareCollisionService : ICollisionService
    {
        private readonly CellCollisionService _cellCollision;
        private readonly HippoGridInteractor  _interactor;
        private readonly IGridService         _grid;

        public DrawingAwareCollisionService(HippoGridInteractor interactor, IGridService grid)
        {
            _cellCollision = new CellCollisionService(grid);
            _interactor    = interactor;
            _grid          = grid;
            Debug.Log("[DrawingAwareCollisionService] Создан");
        }

        public Vector2 ClampToFree(Vector2 from, Vector2 to)
        {
            if (_interactor.IsDrawing)
                return to;

            if (_grid.IsEdge(_grid.WorldToCell(from)))
                return to;

            return _cellCollision.ClampToFree(from, to);
        }
    }
}
