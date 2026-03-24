using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Hippo
{
    /// Принимает ввод, двигает гиппо, держит позицию в пределах границ.
    public class HippoController : MonoBehaviour, IInitializable, IHippoController
    {
        private IInputProvider     _input;
        private IMovementBehaviour _movement;
        private IBoundaryService   _boundary;
        private ICollisionService  _collision;
        private IGridService       _grid;
        private IDrawingState      _drawingState;
        private Vector2            _currentDirection;

        public void Inject(IInputProvider input, IMovementBehaviour movement,
            IBoundaryService boundary, ICollisionService collision, IGridService grid,
            IDrawingState drawingState = null)
        {
            _input        = input;
            _movement     = movement;
            _boundary     = boundary;
            _collision    = collision;
            _grid         = grid;
            _drawingState = drawingState;
            Debug.Log("[HippoController] Inject — все зависимости получены");
        }

        public void Initialize()
        {
            float cell = _grid?.CellSize ?? 0.1f;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

            if (sr.sprite == null)
            {
                Debug.LogError("[HippoController] Sprite не назначен! Назначь спрайт в инспекторе.", this);
                return;
            }

            sr.sortingOrder = 10;

            // Гарантируем scale = 1,1,1 (мог остаться из editor-тестов)
            transform.localScale = Vector3.one;

            PlaceAtSpawn();
            Debug.Log($"[HippoController] Initialize — позиция={transform.position}");
        }

        private void Update()
        {
            if (_input == null || _movement == null || _boundary == null) return;

            Vector2 inputDir = _input.GetDirection();

            if (inputDir != Vector2.zero)
            {
                if (_movement.Stopped)
                    _currentDirection = Vector2.zero;
                else if (IsOpposite(inputDir, _currentDirection))
                    inputDir = Vector2.zero;
            }

            Rect bounds = _boundary.GetBounds();
            _movement.Tick(transform, ref _currentDirection, inputDir, bounds, _collision);
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

        public void SetPosition(Vector3 position) => transform.position = new Vector3(position.x, position.y, -1f);

        public void ResetMovement()
        {
            _currentDirection = Vector2.zero;
            _movement.Resume();
        }

        // Возвращает true если a содержит хотя бы один компонент, противоположный ненулевому компоненту b.
        // Блокирует частичный разворот при диагональном движении (напр. (1,1) → (-1,0)).
        private static bool IsOpposite(Vector2 a, Vector2 b)
        {
            if (a == Vector2.zero || b == Vector2.zero) return false;
            return System.Math.Sign(a.x) == -System.Math.Sign(b.x)
                && System.Math.Sign(a.y) == -System.Math.Sign(b.y);
        }

        private void PlaceAtSpawn()
        {
            Rect bounds = _boundary.GetBounds();
            Vector3 raw  = new Vector3(0f, bounds.yMax, -1f);
            Vector2Int cell    = _grid.WorldToCell(raw);
            Vector2    snapped = _grid.CellToWorld(cell);
            transform.position = new Vector3(snapped.x, snapped.y, -1f);
        }
    }
}
