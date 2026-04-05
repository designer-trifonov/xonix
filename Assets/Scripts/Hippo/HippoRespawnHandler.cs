using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Hippo
{
    /// Фиксирует стартовую точку в момент начала рисования трейла.
    /// При хите — телепортирует обратно на эту точку.
    public class HippoRespawnHandler : MonoBehaviour
    {
        private IHippoGridInteractor _interactor;
        private IHippoController     _hippo;
        private Transform            _hippoTransform;

        private Vector3 _spawnPoint;

        public void Inject(IHippoGridInteractor interactor, IHippoController hippo, Transform hippoTransform)
        {
            _interactor     = interactor;
            _hippo          = hippo;
            _hippoTransform = hippoTransform;

            _interactor.OnHit            += OnHit;
            _interactor.OnDrawingStarted += pos => _spawnPoint = pos;
        }

        public void Initialize()
        {
            _spawnPoint = _hippoTransform.position;
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
