namespace HippoGame.Interfaces
{
    public interface IAudioService
    {
        void Initialize(ILevelManager levelManager,
                        IBallSpawner  ballSpawner,
                        IHippoGridInteractor interactor);
        void PlayButtonClick();
    }
}
