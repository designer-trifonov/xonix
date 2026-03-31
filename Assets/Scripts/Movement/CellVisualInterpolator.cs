using UnityEngine;

namespace HippoGame.Movement
{
    /// Плавная визуальная интерполяция между двумя мировыми позициями.
    /// Не знает о клетках, направлениях или логике движения.
    public class CellVisualInterpolator
    {
        private Vector3 _from;
        private Vector3 _to;
        private float   _t;
        private bool    _active;

        public bool     IsActive    => _active;
        public Vector3  Position    => Vector3.Lerp(_from, _to, _t);

        public void Reset(Vector3 position)
        {
            _from   = position;
            _to     = position;
            _t      = 1f;
            _active = false;
        }

        public void Start(Vector3 from, Vector3 to)
        {
            _from   = from;
            _to     = to;
            _t      = 0f;
            _active = true;
        }

        /// Возвращает true когда интерполяция завершена.
        public bool Advance(float deltaTime, float stepTime)
        {
            if (!_active) return true;

            _t += deltaTime / stepTime;
            if (_t >= 1f)
            {
                _t      = 1f;
                _active = false;
                return true;
            }
            return false;
        }
    }
}
