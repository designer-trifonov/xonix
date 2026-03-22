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
        private Vector2            _currentDirection;

        public void Inject(IInputProvider input, IMovementBehaviour movement,
            IBoundaryService boundary, ICollisionService collision, IGridService grid)
        {
            _input     = input;
            _movement  = movement;
            _boundary  = boundary;
            _collision = collision;
            _grid      = grid;
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
                // Блокируем обратное направление только во время движения.
                // Если стоим — сбрасываем currentDirection, запрет снимается.
                if (_movement.Stopped)
                {
                    _currentDirection = Vector2.zero;
                    Debug.Log($"[HippoController] Ввод {inputDir} | Stopped=true → сброс запрета, принимаем");
                }
                else if (inputDir == -_currentDirection)
                {
                    Debug.Log($"[HippoController] Ввод {inputDir} ЗАБЛОКИРОВАН — обратное направление (current={_currentDirection})");
                    inputDir = Vector2.zero;
                }
                else
                {
                    Debug.Log($"[HippoController] Ввод {inputDir} принят (current={_currentDirection} stopped={_movement.Stopped})");
                }
            }

            Rect bounds = _boundary.GetBounds();
            _movement.Tick(transform, ref _currentDirection, inputDir, bounds, _collision);

            // Снап к центру ячейки по перпендикулярной оси движения
            if (_currentDirection != Vector2.zero && _grid != null)
            {
                Vector3    pos      = transform.position;
                Vector2Int cell     = _grid.WorldToCell(pos);
                Vector2    cellWorld = _grid.CellToWorld(cell);

                if (_currentDirection.x != 0) pos.y = cellWorld.y;
                else                          pos.x = cellWorld.x;

                transform.position = pos;
            }
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
