namespace HippoGame.Interfaces
{
    public interface IGameLogger
    {
        void Log(string msg);
        void Warn(string msg);
        void Error(string msg);
    }
}
