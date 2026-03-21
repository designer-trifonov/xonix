using UnityEngine;

namespace HippoGame.Interfaces
{
    public interface ICollisionService
    {
        Vector2 ClampToFree(Vector2 from, Vector2 to);
    }
}
