using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface ITrailRenderer
    {
        void Inject(Transform hippo, IDrawingState drawingState = null, IGameLogger logger = null);
        void Clear();
    }
}
