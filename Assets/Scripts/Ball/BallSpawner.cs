using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;

namespace HippoGame.Ball
{
    /// Создаёт и уничтожает шары по параметрам уровня.
    /// LevelManager работает с ним только через IBallSpawner.
    public class BallSpawner : MonoBehaviour, IBallSpawner
    {
        private readonly List<IBallController> _balls = new();

        private IGridService      _grid;
        private IBoundaryService  _boundary;
        private Transform         _hippo;
        private IBallInteractable _interactable;

        public void Inject(IGridService grid, IBoundaryService boundary,
            Transform hippo, IBallInteractable interactable)
        {
            _grid         = grid;
            _boundary     = boundary;
            _hippo        = hippo;
            _interactable = interactable;
            Debug.Log("[BallSpawner] Inject — все зависимости получены");
        }

        public void SpawnBalls(int count, float speed)
        {
            ClearBalls();
            Rect bounds = _boundary.GetBounds();
            Debug.Log($"[BallSpawner] SpawnBalls count={count} speed={speed:F2}");

            for (int i = 0; i < count; i++)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"Ball_{i}";
                go.transform.localScale = Vector3.one * 0.25f;
                Destroy(go.GetComponent<Collider>());
                go.GetComponent<Renderer>().material.color = new Color(1f, 0.3f, 0.1f);

                Vector2 pos = RandomInteriorPos(bounds);
                go.transform.position = new Vector3(pos.x, pos.y, -0.5f);

                float dx = Random.value > 0.5f ? 1f : -1f;
                float dy = Random.value > 0.5f ? 1f : -1f;

                BallController ball = go.AddComponent<BallController>();
                ball.Init(new Vector2(dx, dy).normalized, speed, bounds, _grid, _hippo, _interactable);
                _balls.Add(ball);
                Debug.Log($"[BallSpawner] Ball_{i} создан в ({pos.x:F2},{pos.y:F2})");
            }
        }

        public bool CheckBallsAfterFill()
        {
            List<IBallController> caught = new();

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
            return _balls.Count == 0;
        }

        public void ClearBalls()
        {
            Debug.Log($"[BallSpawner] ClearBalls count={_balls.Count}");
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
