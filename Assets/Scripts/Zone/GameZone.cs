using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Zone
{
    /// Физические границы игрового поля.
    [ExecuteAlways]
    public class GameZone : MonoBehaviour, IBoundaryService
    {
        [SerializeField] private Vector2 _zoneSize = Vector2.zero;

        public Rect GetBounds()
        {
            Vector2 center = transform.position;
            return new Rect(center.x - _zoneSize.x / 2f, center.y - _zoneSize.y / 2f, _zoneSize.x, _zoneSize.y);
        }

#if UNITY_EDITOR
        private void OnValidate() => UnityEditor.SceneView.RepaintAll();

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(_zoneSize.x, _zoneSize.y, 0f));
        }
#endif
    }
}
