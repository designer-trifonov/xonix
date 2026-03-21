using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Hippo
{
    /// Принимает ввод, двигает гиппо, держит позицию в пределах границ.
    public class HippoController : MonoBehaviour, IInitializable
    {
        private IInputProvider    _input;
        private IMovementBehaviour _movement;
        private IBoundaryService  _boundary;
        private ICollisionService _collision;
        private Vector2           _currentDirection;

        public void Inject(IInputProvider input, IMovementBehaviour movement,
            IBoundaryService boundary, ICollisionService collision)
        {
            _input     = input;
            _movement  = movement;
            _boundary  = boundary;
            _collision = collision;
            Debug.Log("[HippoController] Inject — все зависимости получены");
        }

        public void Initialize()
        {
            PlaceAtSpawn();
            Debug.Log($"[HippoController] Initialize — позиция={transform.position}");
        }

        private void Update()
        {
            if (_input == null || _movement == null || _boundary == null) return;

            Vector2 inputDir = _input.GetDirection();
            Rect    bounds   = _boundary.GetBounds();
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
            if (_boundary == null)
                _boundary = FindObjectOfType<Zone.GameZone>();

            if (_boundary != null)
                PlaceAtSpawn();
        }

        private void PlaceAtSpawn()
        {
            Rect bounds = _boundary.GetBounds();
            transform.position = new Vector3(0f, bounds.yMax, 0f);
        }
    }
}
