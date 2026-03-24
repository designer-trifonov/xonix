using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Movement;

namespace HippoGame.Hippo
{
    /// Оркестратор: читает ввод → проверяет через DirectionGuard → передаёт в CellMovement.
    public class HippoController : MonoBehaviour, IInitializable, IHippoController
    {
        private IInputProvider   _input;
        private CellMovement     _movement;
        private IBoundaryService _boundary;
        private IGridService     _grid;

        public void Inject(IInputProvider input, IMovementBehaviour movement,
            IBoundaryService boundary, ICollisionService collision, IGridService grid,
            IDrawingState drawingState = null)
        {
            _input    = input;
            _movement = movement as CellMovement;
            _boundary = boundary;
            _grid     = grid;
        }

        public void Initialize()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

            if (sr.sprite == null)
                Debug.LogWarning("[HippoController] Sprite не назначен!", this);
            else
                sr.sortingOrder = 10;

            transform.localScale = Vector3.one;
            PlaceAtSpawn();
        }

        private void Update()
        {
            if (_input == null || _movement == null || _boundary == null) return;

            Vector2Int input = _input.GetDirection();

            // Проверка: можно ли сменить направление?
            if (input != Vector2Int.zero && !DirectionGuard.IsReverse(_movement.Direction, input))
                _movement.QueueDirection(input);

            _movement.Tick(transform, _boundary.GetBounds());
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                SnapToSpawn();
            };
        }
#endif

        public void SnapToSpawn()
        {
#if UNITY_EDITOR
            if (_boundary == null)
                _boundary = FindObjectOfType<Zone.GameZone>();
#endif
            if (_boundary == null || _grid == null) return;
            PlaceAtSpawn();
        }

        public void SetPosition(Vector3 position) =>
            transform.position = new Vector3(position.x, position.y, -1f);

        public void ResetMovement()
        {
            _movement.Resume(transform);
        }

        private void PlaceAtSpawn()
        {
            Rect       bounds  = _boundary.GetBounds();
            Vector3    raw     = new Vector3(0f, bounds.yMax, -1f);
            Vector2Int cell    = _grid.WorldToCell(raw);
            Vector2    snapped = _grid.CellToWorld(cell);
            transform.position = new Vector3(snapped.x, snapped.y, -1f);
        }
    }
}
