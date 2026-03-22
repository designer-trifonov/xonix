using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface IFillService
    {
        void Fill(IGridService grid, List<Vector2Int> trail, IReadOnlyList<Vector2> ballPositions);
    }
}
