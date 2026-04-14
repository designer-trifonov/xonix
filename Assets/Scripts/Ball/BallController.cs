using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Ball
{
    /// Движение шара, отскок от стен и залитых пикселей, попадание в гиппо и трейл.
    public class BallController : MonoBehaviour, IBallController
    {
        public bool    IsAlive  => _active;
        public Vector3 Position => transform.position;

        public event System.Action OnBounce;

        private float _bounceCooldown;
        private const float BounceCooldownTime = 0.1f;

        public void Kill()
        {
            _active = false;
            _renderer.enabled = false;
        }

        public void SetSpeed(float speed)    => _speed = speed;
        public void SetVisible(bool visible) => _renderer.enabled = visible;

        private Renderer          _renderer;
        private bool              _active;
        private Vector2           _direction;
        private float             _speed;
        private Rect              _bounds;
        private IGridService      _grid;
        private Transform         _hippo;
        private IBallInteractable _interactable;

        private const float HippoHitRadius = 0.2f;
        private const float TrailHitRadius = 0.08f;

        public void Init(Vector2 direction, float speed, Rect bounds,
            IGridService grid, Transform hippo, IBallInteractable interactable)
        {
            _renderer     = GetComponent<Renderer>();
            _active       = true;
            _renderer.enabled = true;
            _direction    = direction.normalized;
            _speed        = speed;
            _bounds       = bounds;
            _grid         = grid;
            _hippo        = hippo;
            _interactable = interactable;
            Debug.Log($"[BallController] Init | speed={speed:F2} dir=({_direction.x:F1},{_direction.y:F1})");
        }

        private void Update()
        {
            if (!_active || _interactable == null) return;

            Vector2 pos  = transform.position;
            Vector2 next = pos + _direction * _speed * Time.deltaTime;

            _bounceCooldown -= Time.deltaTime;

            bool bounced = false;
            if (next.x <= _bounds.xMin || next.x >= _bounds.xMax)
            {
                _direction.x = -_direction.x;
                next.x = Mathf.Clamp(next.x, _bounds.xMin, _bounds.xMax);
                bounced = true;
            }
            if (next.y <= _bounds.yMin || next.y >= _bounds.yMax)
            {
                _direction.y = -_direction.y;
                next.y = Mathf.Clamp(next.y, _bounds.yMin, _bounds.yMax);
                bounced = true;
            }
            if (bounced) FireBounce();

            BounceOffFilled(pos, ref next);

            transform.position = new Vector3(next.x, next.y, -0.5f);

            if (!_interactable.IsVulnerable) return;

            if (Vector2.Distance(next, _hippo.position) < HippoHitRadius)
            {
                Debug.Log("[BallController] Попадание в гиппо!");
                _interactable.OnBallHit(next);
                return;
            }

            if (_interactable.IsNearTrail(next, TrailHitRadius))
            {
                Debug.Log("[BallController] Попадание в трейл!");
                _interactable.OnBallHit(next);
            }
        }

        private void BounceOffFilled(Vector2 from, ref Vector2 to)
        {
            Vector2Int toCell = _grid.WorldToCell(to);
            if (!_grid.IsInBounds(toCell)) return;
            if (_grid.GetCell(toCell.x, toCell.y) != CellState.Filled) return;

            Vector2Int xCell = _grid.WorldToCell(new Vector2(to.x, from.y));
            Vector2Int yCell = _grid.WorldToCell(new Vector2(from.x, to.y));

            bool xFilled = _grid.IsInBounds(xCell) && _grid.GetCell(xCell.x, xCell.y) == CellState.Filled;
            bool yFilled = _grid.IsInBounds(yCell) && _grid.GetCell(yCell.x, yCell.y) == CellState.Filled;

            if (xFilled) { _direction.x = -_direction.x; to.x = from.x; }
            if (yFilled) { _direction.y = -_direction.y; to.y = from.y; }
            if (!xFilled && !yFilled) { _direction = -_direction; to = from; }

            FireBounce();
        }

        private void FireBounce()
        {
            if (_bounceCooldown > 0f) return;
            _bounceCooldown = BounceCooldownTime;
            OnBounce?.Invoke();
        }
    }
}
