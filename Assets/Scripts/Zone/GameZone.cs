using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Core;

namespace HippoGame.Zone
{
    /// Физические границы игрового поля.
    /// Отдаёт Rect на основе размера из GameManager.
    [ExecuteAlways]
    public class GameZone : MonoBehaviour, IBoundaryService
    {
        public Rect GetBounds()
        {
            if (GameManager.Instance == null)
                throw new System.InvalidOperationException("[GameZone] GameManager.Instance is null");

            Vector2 size   = GameManager.Instance.ZoneSize;
            Vector2 center = transform.position;
            return new Rect(center.x - size.x / 2f, center.y - size.y / 2f, size.x, size.y);
        }

#if UNITY_EDITOR
        private void OnValidate() => UnityEditor.SceneView.RepaintAll();

        private void OnDrawGizmos()
        {
            if (GameManager.Instance == null) return;
            Gizmos.color = Color.green;
            Vector2 size = GameManager.Instance.ZoneSize;
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
        }
#endif
    }
}
