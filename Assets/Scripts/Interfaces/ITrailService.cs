using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface ITrailService
    {
        IReadOnlyList<Vector2Int> Points { get; }
        void AddPoint(Vector2Int cell);
        void Clear();
    }
}
