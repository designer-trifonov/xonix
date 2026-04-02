namespace HippoGame.Interfaces
{
    public interface IDrawingState
    {
        bool IsDrawing    { get; }
        bool IsHitInProgress { get; }
    }
}
