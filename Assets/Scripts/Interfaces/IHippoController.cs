using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IHippoController
    {
        void SetPosition(Vector3 position);
        void ResetMovement();
    }
}
