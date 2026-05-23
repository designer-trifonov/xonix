using HippoGame.Core;

namespace HippoGame.Interfaces
{
    public interface ISaveManager
    {
        bool HasSavedGame { get; }

        void         Save(GameSnapshot snap);
        GameSnapshot LoadSnapshot();
        void         ClearSave();
    }
}
