namespace HippoGame.Interfaces
{
    /// Контракт спаунера шаров:
    /// LevelManager управляет шарами только через этот интерфейс.
    public interface IBallSpawner
    {
        void SpawnBalls(int count, float speed);
        bool CheckBallsAfterFill();
        void ClearBalls();
    }
}
