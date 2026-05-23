using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IHippoController
    {
        Vector3 Position { get; }
        void SetPosition(Vector3 position);
        void ResetMovement();
        void SetVisible(bool visible);
    }
}
