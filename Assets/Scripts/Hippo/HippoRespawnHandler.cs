using UnityEngine;
using HippoGame.Grid;
using HippoGame.Interfaces;

namespace HippoGame.Hippo
{
    /// Запоминает позицию гиппо на edge-клетке перед началом рисования.
    /// При хите — телепортирует обратно и сбрасывает состояние.
    public class HippoRespawnHandler : MonoBehaviour
    {
        [SerializeField] private HippoGridInteractor _interactor;
        [SerializeField] private HippoController     _hippo;
        [SerializeField] private GameGrid            _grid;

        private Vector3 _spawnPoint;

        private void Awake()
        {
            _interactor.OnHit += OnHit;
        }

        public void Initialize()
        {
            _spawnPoint = _hippo.transform.position;
        }

        private void Update()
        {
            if (_interactor.IsDrawing) return;

            // Сохраняем позицию только когда гиппо стоит на edge-клетке
            Vector2Int cell = _grid.WorldToCell(_hippo.transform.position);
            if (_grid.IsEdge(cell))
            {
                Vector2 world = _grid.CellToWorld(cell);
                _spawnPoint = new Vector3(world.x, world.y, _hippo.transform.position.z);
            }
        }

        private void OnHit()
        {
            _hippo.SetPosition(_spawnPoint);
            _interactor.ResetState(_spawnPoint);
            _hippo.ResetMovement();
        }
    }
}
