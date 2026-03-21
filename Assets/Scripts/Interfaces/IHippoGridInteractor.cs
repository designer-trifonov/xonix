using System;
using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IHippoGridInteractor
    {
        event Action OnZoneFilled;
        event Action OnHit;
        void ResetState(Vector3 hippoPosition);
    }
}
