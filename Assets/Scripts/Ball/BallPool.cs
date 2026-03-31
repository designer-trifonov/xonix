using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Ball
{
    /// Пул шаров: создаёт и переиспользует BallController объекты.
    /// Не знает о скорости, позиции, игровом состоянии.
    public class BallPool
    {
        private readonly List<BallController> _pool = new();
        private readonly GameObject           _prefab;

        public BallPool(GameObject prefab)
        {
            _prefab = prefab;
        }

        public BallController Get()
        {
            BallController found = FindInactive();
            if (found != null) return found;

            GameObject go = _prefab != null
                ? Object.Instantiate(_prefab)
                : CreateFallback();
            go.name = $"Ball_{_pool.Count}";

            BallController ball = go.AddComponent<BallController>();
            _pool.Add(ball);
            Debug.Log($"[BallPool] Создан новый шар, размер пула={_pool.Count}");
            return ball;
        }

        private BallController FindInactive()
        {
            foreach (var b in _pool)
                if (!b.IsAlive) return b;
            return null;
        }

        private static GameObject CreateFallback()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = Vector3.one * 0.25f;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().material.color = new Color(1f, 0.3f, 0.1f);
            return go;
        }
    }
}
