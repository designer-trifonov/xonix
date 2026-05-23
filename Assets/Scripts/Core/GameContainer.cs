using HippoGame.Interfaces;
using HippoGame.Zone;
using HippoGame.Input;
using HippoGame.Movement;
using HippoGame.Grid;
using HippoGame.Hippo;
using HippoGame.Ball;
using HippoGame.FX;
using HippoGame.Trail;
using HippoGame.Ads;
using UnityEngine;

namespace HippoGame.Core
{
    /// Строит и регистрирует все зависимости.
    /// Bootstrap предоставляет MonoBehaviour-ссылки, этот класс их регистрирует.
    public static class GameContainer
    {
        public static DiContainer Build(
            LevelConfig          levelConfig,
            GameZone             gameZone,
            HippoController      hippoController,
            GameGrid             gameGrid,
            HippoGridInteractor  hippoGridInteractor,
            ParticleEffectsService particleEffects,
            BallSpawner          ballSpawner,
            AdController         adController,
            TrailLineRenderer    trailLineRenderer = null)
        {
            var container = new DiContainer();

            var gameState = new GameState(levelConfig);
            container.Register<GameState>(gameState);
            container.Register<IGameState>(gameState);

            container.Register<IBoundaryService>(gameZone);
            container.Register<IAdService>(adController);
            container.Register<IInputProvider>(new CombinedInputProvider());
            container.Register<HippoController>(hippoController);
            container.Register<IHippoController>(hippoController);
            container.Register<GameGrid>(gameGrid);
            container.Register<IGridService>(gameGrid);
            container.Register<IGridRenderer>(gameGrid);

            var movement = new CellMovement(container.Resolve<IGridService>());
            container.Register<CellMovement>(movement);
            container.Register<IMovementBehaviour>(movement);

            container.Register<IFillService>(new FloodFillService());

            var grid = container.Resolve<IGridService>();
            container.Register<ICollisionService>(new DrawingAwareCollisionService(
                hippoGridInteractor, grid, new CellCollisionService(grid)));

            container.Register<IHippoGridInteractor>(hippoGridInteractor);
            container.Register<HippoGridInteractor>(hippoGridInteractor);
            container.Register<ITrailService>(new TrailTracker());
            container.Register<IBallSpawner>(ballSpawner);
            container.Register<IBallInteractable>(hippoGridInteractor);

            if (particleEffects != null)
                container.Register<IParticleService>(particleEffects);

            if (trailLineRenderer != null)
                container.Register<ITrailRenderer>(trailLineRenderer);

            var levelManager = new LevelManager();
            container.Register<LevelManager>(levelManager);
            container.Register<ILevelManager>(levelManager);

            var restorer = new GameRestorer();
            container.Register<GameRestorer>(restorer);
            container.Register<IGameRestorer>(restorer);

            container.Register<IGameLogger>(new GameLogger());
            container.Register<IPauseService>(new PauseService());

            return container;
        }
    }
}
