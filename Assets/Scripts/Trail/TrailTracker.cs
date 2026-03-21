using System.Collections.Generic;
using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Trail
{
    /// Хранит список клеток текущего хвоста гиппо.
    /// Чистая модель — никакой логики отрисовки или сетки.
    public class TrailTracker : ITrailService
    {
        private readonly List<Vector2Int> _points = new();

        public IReadOnlyList<Vector2Int> Points => _points;

        public void AddPoint(Vector2Int cell)
        {
            _points.Add(cell);
            Debug.Log($"[TrailTracker] AddPoint ({cell.x},{cell.y}) | total={_points.Count}");
        }

        public void Clear()
        {
            Debug.Log($"[TrailTracker] Clear | было {_points.Count} точек");
            _points.Clear();
        }
    }
}
