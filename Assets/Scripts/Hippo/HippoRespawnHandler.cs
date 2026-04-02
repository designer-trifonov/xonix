using UnityEngine;
using HippoGame.Hippo;
using HippoGame.Interfaces;

namespace HippoGame.Hippo
{
    /// Запоминает позицию гиппо в момент начала рисования.
    /// При хите — телепортирует обратно в эту точку.
    /// Другие скрипты не трогает. Подключается через инспектор.
    public class HippoRespawnHandler : MonoBehaviour
    {
        [SerializeField] private HippoGridInteractor _interactor;
        [SerializeField] private HippoController     _hippo;

        private bool    _wasDrawing;
        private Vector3 _spawnPoint;

        private void Awake()
        {
            _spawnPoint = _hippo.transform.position;
            _interactor.OnHit += OnHit;
        }

        private void Update()
        {
            bool isDrawing = _interactor.IsDrawing;
            if (isDrawing && !_wasDrawing)
                _spawnPoint = _hippo.transform.position;
            _wasDrawing = isDrawing;
        }

        private void OnHit()
        {
            _hippo.SetPosition(_spawnPoint);
        }
    }
}
