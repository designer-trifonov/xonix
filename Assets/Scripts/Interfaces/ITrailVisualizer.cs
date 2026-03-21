using System.Collections.Generic;
using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface ITrailVisualizer
    {
        void Redraw(IReadOnlyList<Vector2Int> points);
        /// Рисует committed-трейл + одну preview-клетку (текущая позиция гиппо).
        void RedrawWithPreview(IReadOnlyList<Vector2Int> points, Vector2Int previewCell);
        void Clear();
    }
}
