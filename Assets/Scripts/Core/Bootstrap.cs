using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Zone;
using HippoGame.Input;
using HippoGame.Movement;
using HippoGame.Grid;
using HippoGame.Hippo;
using HippoGame.Ball;
using HippoGame.UI;
using HippoGame.FX;
using HippoGame.Trail;
using HippoGame.Ads;

namespace HippoGame.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private LevelConfig _levelConfig;

        [Header("Systems")]
        [SerializeField] private GameZone              _gameZone;
        [SerializeField] private HippoController       _hippoController;
        [SerializeField] private GameGrid              _gameGrid;
        [SerializeField] private HippoGridInteractor   _hippoGridInteractor;
        [SerializeField] private ParticleEffectsService _particleEffects;
        [SerializeField] private BallSpawner           _ballSpawner;
        [SerializeField] private AdController          _adController;
        [SerializeField] private GameStartController   _gameStartController;
        [SerializeField] private GameStartAdHandler    _gameStartAdHandler;
        [SerializeField] private HippoRespawnHandler   _hippoRespawnHandler;
        [SerializeField] private LevelColorController  _levelColorController;

        [Header("UI")]
        [SerializeField] private LivesUIController       _livesUI;
        [SerializeField] private ScoreUIController       _scoreUI;
        [SerializeField] private LevelUIController       _levelUI;
        [SerializeField] private FillPercentUIController _percentUI;
        [SerializeField] private GameOverUIController    _gameOverUI;
        [SerializeField] private ShopUIController        _shopUI;

        private void Awake()
        {
            var container = GameContainer.Build(
                _levelConfig,
                _gameZone,
                _hippoController,
                _gameGrid,
                _hippoGridInteractor,
                _particleEffects,
                _ballSpawner,
                _adController);
            Inject(container);
        }

        private void Inject(DiContainer container)
        {
            var state        = container.Resolve<GameState>();
            var grid         = container.Resolve<IGridService>();
            var levelManager = container.Resolve<LevelManager>();

            // ── Inject ───────────────────────────────────────────────────────────
            _hippoController.Inject(
                container.Resolve<IInputProvider>(),
                container.Resolve<IMovementBehaviour>(),
                container.Resolve<IBoundaryService>(),
                container.Resolve<ICollisionService>(),
                container.Resolve<IGridService>(),
                _hippoGridInteractor);

            _hippoGridInteractor.Inject(
                grid,
                container.Resolve<IFillService>(),
                container.Resolve<ITrailService>(),
                _hippoController.transform,
                container.Resolve<IMovementBehaviour>(),
                container.TryResolve<IParticleService>(),
                container.Resolve<IBallSpawner>(),
                container.Resolve<IGameLogger>());

            _ballSpawner.Inject(
                grid,
                container.Resolve<IBoundaryService>(),
                _hippoController.transform,
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

            _hippoRespawnHandler.Inject(
                container.Resolve<IHippoGridInteractor>(),
                container.Resolve<IHippoController>(),
                _hippoController.transform);
            levelManager.SetRespawnAction(_hippoRespawnHandler.Respawn);

            _levelColorController.Inject(container.Resolve<IGameState>());

            _livesUI.Inject(state);
            _scoreUI.Inject(state);
            _levelUI.Inject(state);
            _percentUI.Inject(state, grid);
            _shopUI.Inject(state, container.Resolve<IBallSpawner>(), container.Resolve<IAdService>(), container.Resolve<IPauseService>());

            _gameOverUI.Inject(container.Resolve<IPauseService>());
            levelManager.OnGameOver      += _gameOverUI.Show;
            _gameOverUI.OnRestart        += levelManager.RestartFromLevel1;
            _gameOverUI.OnWatchAd        += levelManager.ContinueAfterAd;

            _gameStartController.Inject(container.Resolve<IGameState>());
            _gameStartAdHandler.Inject(container.Resolve<IAdService>());
            _gameStartAdHandler.OnCompleted += _gameStartController.BeginFlow;

            _hippoGridInteractor.OnHit += () => container.Resolve<IGameState>().LoseLife();

            // ── Initialize (строго по порядку) ───────────────────────────────────
            _gameStartController.RegisterInit(_gameGrid.Initialize);
            _gameStartController.RegisterInit(_hippoController.Initialize);
            _gameStartController.RegisterInit(_hippoRespawnHandler.Initialize);
            _gameStartController.RegisterInit(_levelColorController.Initialize);
            _gameStartController.RegisterInit(_hippoGridInteractor.Initialize);
            _gameStartController.RegisterInit(levelManager.Initialize);
            _gameStartController.RegisterInit(_livesUI.Initialize);
            _gameStartController.RegisterInit(_scoreUI.Initialize);
            _gameStartController.RegisterInit(_levelUI.Initialize);
            _gameStartController.RegisterInit(_percentUI.Initialize);
        }

    }
}
