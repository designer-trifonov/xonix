using UnityEngine;
using HippoGame.Interfaces;

namespace HippoGame.Core
{
    public class PauseService : IPauseService
    {
        public void Pause()  => Time.timeScale = 0f;
        public void Resume() => Time.timeScale = 1f;
    }
}
