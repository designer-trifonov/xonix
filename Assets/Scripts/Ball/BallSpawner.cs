using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Ball
{
    /// Создаёт и переиспользует шары через пул.
    public class BallSpawner : MonoBehaviour, IBallSpawner
    {
        [SerializeField] private GameObject _ballPrefab;

        private BallPool               _pool;
        private readonly List<IBallController> _balls = new();

        private IGridService      _grid;
        private IBoundaryService  _boundary;
        private Transform         _hippo;
        private IBallInteractable _interactable;
        private float             _currentBallSpeed;
        private float             _originalBallSpeed;

        public void Inject(IGridService grid, IBoundaryService boundary,
            Transform hippo, IBallInteractable interactable)
        {
            _grid         = grid;
            _boundary     = boundary;
            _hippo        = hippo;
            _interactable = interactable;
            _pool         = new BallPool(_ballPrefab);
            Debug.Log("[BallSpawner] Inject — все зависимости получены");
        }

        public void SpawnBalls(int count, float speed)
        {
            ClearBalls();
            _currentBallSpeed = speed;
            _originalBallSpeed = speed;
            Rect bounds = _boundary.GetBounds();
            Debug.Log($"[BallSpawner] SpawnBalls count={count} speed={speed:F2}");

            for (int i = 0; i < count; i++)
            {
                BallController ball = _pool.Get();

                Vector2 pos = RandomInteriorPos(bounds);
                ball.transform.position = new Vector3(pos.x, pos.y, -0.5f);

                float dx = Random.value > 0.5f ? 1f : -1f;
                float dy = Random.value > 0.5f ? 1f : -1f;
                ball.Init(new Vector2(dx, dy).normalized, speed, bounds, _grid, _hippo, _interactable);

                ball.gameObject.SetActive(true);
                _balls.Add(ball);
                Debug.Log($"[BallSpawner] Ball_{i} активирован в ({pos.x:F2},{pos.y:F2})");
            }
        }

        public int CheckBallsAfterFill()
        {
            var caught = new List<IBallController>();

            foreach (var ball in _balls)
            {
                if (!ball.IsAlive) continue;
                Vector2Int cell = _grid.WorldToCell(ball.Position);
                if (_grid.GetCell(cell.x, cell.y) == CellState.Filled)
                    caught.Add(ball);
            }

            foreach (var ball in caught)
            {
                _balls.Remove(ball);
                ball.Kill();
            }

            Debug.Log($"[BallSpawner] CheckBallsAfterFill: поймано={caught.Count} осталось={_balls.Count}");
            return caught.Count;
        }

        public IReadOnlyList<Vector2> GetPositions()
        {
            var positions = new List<Vector2>(_balls.Count);
            foreach (var b in _balls)
                if (b.IsAlive) positions.Add(b.Position);
            return positions;
        }

        public bool RemoveOneBall()
        {
            for (int i = _balls.Count - 1; i >= 0; i--)
            {
                if (!_balls[i].IsAlive) continue;
                if (_balls.Count <= 1)
                {
                    Debug.Log("[BallSpawner] RemoveOneBall: защита — минимум 1 шар");
                    return false;
                }
                _balls[i].Kill();
                _balls.RemoveAt(i);
                Debug.Log($"[BallSpawner] RemoveOneBall: шар возвращён в пул, осталось={_balls.Count}");
                return true;
            }
            return false;
        }

        public void SlowBalls(float factor)
        {
            float minSpeed = _originalBallSpeed * 0.2f;
            _currentBallSpeed = Mathf.Max(_currentBallSpeed * factor, minSpeed);
            foreach (var ball in _balls)
                if (ball.IsAlive) ball.SetSpeed(_currentBallSpeed);
            Debug.Log($"[BallSpawner] SlowBalls: скорость={_currentBallSpeed:F2} (мин={minSpeed:F2})");
        }

        public void SetBallsVisible(bool visible)
        {
            foreach (var b in _balls)
                if (b.IsAlive) b.SetVisible(visible);
        }

        public void ClearBalls()
        {
            Debug.Log($"[BallSpawner] ClearBalls: возвращаем {_balls.Count} шаров в пул");
            foreach (var b in _balls)
                if (b.IsAlive) b.Kill();
            _balls.Clear();
        }

        private Vector2 RandomInteriorPos(Rect bounds)
        {
            float margin = Mathf.Min(bounds.width, bounds.height) * 0.15f;
            return new Vector2(
                Random.Range(bounds.xMin + margin, bounds.xMax - margin),
                Random.Range(bounds.yMin + margin, bounds.yMax - margin)
            );
        }
    }
}
