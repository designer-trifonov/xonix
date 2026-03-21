using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface ITrailRenderer
    {
        void Inject(Transform hippo);
        void Clear();
    }
}
