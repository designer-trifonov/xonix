using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Hippo
{
    /// Фиксирует стартовую точку в момент начала рисования трейла.
    /// При хите — телепортирует обратно на эту точку.
    public class HippoRespawnHandler : MonoBehaviour
    {
        [SerializeField] private HippoGridInteractor _interactor;
        [SerializeField] private HippoController     _hippo;

        private Vector3 _spawnPoint;

        private void Awake()
        {
            _interactor.OnHit            += OnHit;
            _interactor.OnDrawingStarted += pos => _spawnPoint = pos;
        }

        public void Initialize()
        {
            _spawnPoint = _hippo.transform.position;
        }

        public void Respawn()
        {
            _hippo.SetPosition(_spawnPoint);
            _interactor.ResetState(_spawnPoint);
            _hippo.ResetMovement();
        }

        private void OnHit() => Respawn();
    }
}
