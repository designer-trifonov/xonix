using System;
using HippoGame.Core;

namespace HippoGame.Interfaces
{
    public interface ISnapshotCollector
    {
        event Action<GameSnapshot> OnSnapshot;

        void Activate();
        void Deactivate();
        void Capture();
    }
}
