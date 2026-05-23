using UnityEngine;
using YG;
using HippoGame.Interfaces;
using HippoGame.Grid;
using HippoGame.Hippo;
using HippoGame.Ball;
using HippoGame.Movement;
using HippoGame.Trail;
using HippoGame.UI;
using HippoGame.Ads;
using HippoGame.Audio;

namespace HippoGame.Core
{
    public static class GameInjector
    {
        public static void Inject(
            DiContainer              container,
            HippoController          hippoController,
            HippoGridInteractor      hippoGridInteractor,
            BallSpawner              ballSpawner,
            TrailLineRenderer        trailLineRenderer,
            HippoRespawnHandler      hippoRespawnHandler,
            LevelColorController     levelColorController,
            LivesUIController        livesUI,
            ScoreUIController        scoreUI,
            LevelUIController        levelUI,
            FillPercentUIController  percentUI,
            GameOverUIController     gameOverUI,
            ShopUIController         shopUI,
            GameStartController      gameStartController,
            GameStartAdHandler       gameStartAdHandler,
            LeaderboardUIController  leaderboardUI,
            AudioService             audioService      = null,
            SaveManager              saveManager       = null,
            GameSnapshotCollector    snapshotCollector = null)
        {
            var state        = container.Resolve<GameState>();
            var grid         = container.Resolve<IGridService>();
            var levelManager = container.Resolve<LevelManager>();
            var restorer     = container.Resolve<GameRestorer>();

            // ── Игровые системы ───────────────────────────────────────────────────────

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

            trailLineRenderer?.Inject(
                hippoController.transform,
                hippoGridInteractor,
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

            // ── Сохранения ────────────────────────────────────────────────────────────

            restorer.Inject(
                grid,
                container.Resolve<IHippoController>(),
                container.Resolve<IMovementBehaviour>(),
                container.Resolve<IHippoGridInteractor>(),
                container.Resolve<ILevelManager>(),
                container.Resolve<IBallSpawner>(),
                container.TryResolve<ITrailRenderer>());

            if (snapshotCollector != null)
            {
                snapshotCollector.Inject(
                    container.Resolve<IGameState>(),
                    grid,
                    container.Resolve<ILevelManager>(),
                    container.Resolve<IHippoController>(),
                    container.Resolve<IMovementBehaviour>(),
                    container.Resolve<IHippoGridInteractor>(),
                    container.Resolve<ITrailService>(),
                    container.Resolve<IBallSpawner>());

                // Коллектор → SaveManager: единственная точка сохранения
                if (saveManager != null)
                    snapshotCollector.OnSnapshot += saveManager.Save;
            }

            // ── UI ────────────────────────────────────────────────────────────────────

            levelColorController.Inject(container.Resolve<IGameState>());
            livesUI.Inject(state);
            scoreUI.Inject(state);
            levelUI.Inject(state);
            percentUI.Inject(state, grid);
            shopUI.Inject(state, container.Resolve<IBallSpawner>(),
                container.Resolve<IAdService>(),
                container.Resolve<IPauseService>(),
                container.Resolve<IGameLogger>());
            leaderboardUI.Inject(state);

            gameOverUI.Inject(container.Resolve<IPauseService>());

            // ── Game Over ─────────────────────────────────────────────────────────────

            levelManager.OnGameOver += gameOverUI.Show;
            levelManager.OnGameOver += leaderboardUI.Show;
            levelManager.OnGameOver += () =>
            {
                snapshotCollector?.Deactivate();
                saveManager?.ClearSave();
            };

            gameOverUI.OnRestart += () =>
            {
                gameOverUI.Hide();
                leaderboardUI.Hide();
                levelManager.ClearFieldVisuals();
                gameStartController.ShowDifficultyForRestart(() =>
                {
                    gameOverUI.Hide();
                    leaderboardUI.Initialize();
                    levelManager.RestartFromLevel1();
                    snapshotCollector?.Activate();
                });
            };

            gameOverUI.OnWatchAd += levelManager.ContinueAfterAd;

            // ── Старт / главное меню ──────────────────────────────────────────────────

            gameStartController.Inject(
                container.Resolve<IGameState>(),
                saveManager, snapshotCollector, restorer);

            gameStartController.SetMainMenuAction(() =>
            {
                snapshotCollector?.Deactivate();
                saveManager?.ClearSave();
                levelManager.ClearFieldVisuals();
                gameStartController.ShowDifficultyForRestart(() =>
                {
                    levelManager.RestartFromLevel1();
                    snapshotCollector?.Activate();
                });
            });

            gameStartAdHandler.Inject(container.Resolve<IAdService>());
            gameStartAdHandler.OnCompleted += gameStartController.BeginFlow;

            hippoGridInteractor.OnHit += () => container.Resolve<IGameState>().LoseLife();

            if (audioService != null)
                audioService.Initialize(
                    container.Resolve<ILevelManager>(),
                    container.Resolve<IBallSpawner>(),
                    container.Resolve<IHippoGridInteractor>());

            // ── Порядок инициализации ─────────────────────────────────────────────────

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
            gameStartController.RegisterInit(leaderboardUI.Initialize);
        }
    }
}
