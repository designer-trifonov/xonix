using System;
using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IHippoGridInteractor
    {
        event Action OnZoneFilled;
        event Action OnHit;
        event Action<Vector3> OnDrawingStarted;
        void ResetState(Vector3 hippoPosition);
    }
}
