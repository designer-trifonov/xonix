using System;
using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IHippoGridInteractor
    {
        bool IsDrawing    { get; }
        bool IsVulnerable { get; }

        event Action OnZoneFilled;
        event Action OnHit;
        event Action<Vector3> OnDrawingStarted;

        void ResetState(Vector3 hippoPosition);
        void RestoreDrawingState(Vector2Int lastCell, bool isDrawing, IReadOnlyList<Vector2Int> trailPoints);
    }
}
