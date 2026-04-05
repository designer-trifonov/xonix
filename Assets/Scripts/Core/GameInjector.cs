using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Grid;
using HippoGame.Hippo;
using HippoGame.Ball;
using HippoGame.UI;
using HippoGame.Ads;

namespace HippoGame.Core
{
    public static class GameInjector
    {
        public static void Inject(
            DiContainer             container,
            HippoController         hippoController,
            HippoGridInteractor     hippoGridInteractor,
            BallSpawner             ballSpawner,
            HippoRespawnHandler     hippoRespawnHandler,
            LevelColorController    levelColorController,
            LivesUIController       livesUI,
            ScoreUIController       scoreUI,
            LevelUIController       levelUI,
            FillPercentUIController percentUI,
            GameOverUIController    gameOverUI,
            ShopUIController        shopUI,
            GameStartController     gameStartController,
            GameStartAdHandler      gameStartAdHandler)
        {
            var state        = container.Resolve<GameState>();
            var grid         = container.Resolve<IGridService>();
            var levelManager = container.Resolve<LevelManager>();

            hippoController.Inject(
                container.Resolve<IInputProvider>(),
                container.Resolve<IMovementBehaviour>(),
                container.Resolve<IBoundaryService>(),
                container.Resolve<ICollisionService>(),
                container.Resolve<IGridService>(),
                hippoGridInteractor);

            hippoGridInteractor.Inject(
                grid,
                container.Resolve<IFillService>(),
                container.Resolve<ITrailService>(),
                hippoController.transform,
                container.Resolve<IMovementBehaviour>(),
                container.TryResolve<IParticleService>(),
                container.Resolve<IBallSpawner>(),
                container.Resolve<IGameLogger>());

            ballSpawner.Inject(
                grid,
                container.Resolve<IBoundaryService>(),
                hippoController.transform,
                container.Resolve<IBallInteractable>());

            levelManager.Inject(
                container.Resolve<IGameState>(),
                grid,
                container.Resolve<IBallSpawner>(),
                container.Resolve<IHippoController>(),
                container.Resolve<IHippoGridInteractor>(),
                container.Resolve<IMovementBehaviour>(),
                container.Resolve<IBoundaryService>(),
                container.Resolve<IAdService>());

            hippoRespawnHandler.Inject(
                container.Resolve<IHippoGridInteractor>(),
                container.Resolve<IHippoController>(),
                hippoController.transform);
            levelManager.SetRespawnAction(hippoRespawnHandler.Respawn);

            levelColorController.Inject(container.Resolve<IGameState>());

            livesUI.Inject(state);
            scoreUI.Inject(state);
            levelUI.Inject(state);
            percentUI.Inject(state, grid);
            shopUI.Inject(state, container.Resolve<IBallSpawner>(), container.Resolve<IAdService>(), container.Resolve<IPauseService>());

            gameOverUI.Inject(container.Resolve<IPauseService>());
            levelManager.OnGameOver   += gameOverUI.Show;
            gameOverUI.OnRestart      += levelManager.RestartFromLevel1;
            gameOverUI.OnWatchAd      += levelManager.ContinueAfterAd;

            gameStartController.Inject(container.Resolve<IGameState>());
            gameStartAdHandler.Inject(container.Resolve<IAdService>());
            gameStartAdHandler.OnCompleted += gameStartController.BeginFlow;

            hippoGridInteractor.OnHit += () => container.Resolve<IGameState>().LoseLife();

            gameStartController.RegisterInit(container.Resolve<GameGrid>().Initialize);
            gameStartController.RegisterInit(hippoController.Initialize);
            gameStartController.RegisterInit(hippoRespawnHandler.Initialize);
            gameStartController.RegisterInit(levelColorController.Initialize);
            gameStartController.RegisterInit(hippoGridInteractor.Initialize);
            gameStartController.RegisterInit(levelManager.Initialize);
            gameStartController.RegisterInit(livesUI.Initialize);
            gameStartController.RegisterInit(scoreUI.Initialize);
            gameStartController.RegisterInit(levelUI.Initialize);
            gameStartController.RegisterInit(percentUI.Initialize);
        }
    }
}
