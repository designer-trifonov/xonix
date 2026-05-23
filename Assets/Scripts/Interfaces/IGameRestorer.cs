using HippoGame.Core;

namespace HippoGame.Interfaces
{
    public interface IGameRestorer
    {
        void Restore(GameSnapshot snap);
    }
}
