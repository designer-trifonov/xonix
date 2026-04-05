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
            var container = BuildContainer();
            Inject(container);
        }

        private DiContainer BuildContainer()
        {
            var container = new DiContainer();
            var gameState = new GameState(_levelConfig);
            container.Register<GameState>(gameState);
            container.Register<IGameState>(gameState);
            container.Register<IBoundaryService>(_gameZone);
            container.Register<IAdService>(_adController);
            container.Register<IInputProvider>(new CombinedInputProvider());
            container.Register<HippoController>(_hippoController);
            container.Register<IGridService>(_gameGrid);
            container.Register<IGridRenderer>(_gameGrid);
            var movement = new CellMovement(container.Resolve<IGridService>());
            container.Register<CellMovement>(movement);
            container.Register<IMovementBehaviour>(movement);
            container.Register<IFillService>(new FloodFillService());
            var grid = container.Resolve<IGridService>();
            container.Register<ICollisionService>(new DrawingAwareCollisionService(
                _hippoGridInteractor, grid, new CellCollisionService(grid)));
            container.Register<IHippoController>(_hippoController);
            container.Register<IHippoGridInteractor>(_hippoGridInteractor);
            container.Register<ITrailService>(new TrailTracker());
            container.Register<IBallSpawner>(_ballSpawner);
            container.Register<IBallInteractable>(_hippoGridInteractor);
            if (_particleEffects != null)
                container.Register<IParticleService>(_particleEffects);
            container.Register<HippoGridInteractor>(_hippoGridInteractor);
            container.Register<LevelManager>(new LevelManager());
            container.Register<IGameLogger>(new GameLogger());
            container.Register<IPauseService>(new PauseService());
            return container;
        }

        private void Inject(DiContainer container)
        {
            var state        = container.Resolve<GameState>();
            var grid         = container.Resolve<IGridService>();
            var levelManager = container.Resolve<LevelManager>();

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
                _particleEffects != null ? container.Resolve<IParticleService>() : null,
                container.Resolve<IBallSpawner>(),
                container.Resolve<IGameLogger>());

            _hippoGridInteractor.OnHit += () => container.Resolve<IGameState>().LoseLife();
            if (_hippoRespawnHandler != null)
                levelManager.SetRespawnAction(_hippoRespawnHandler.Respawn);

            if (_ballSpawner != null)
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

            if (_livesUI   != null) _livesUI.Inject(state);
            if (_scoreUI   != null) _scoreUI.Inject(state);
            if (_levelUI   != null) _levelUI.Inject(state);
            if (_percentUI != null) _percentUI.Inject(state, grid);
            if (_shopUI    != null) _shopUI.Inject(state, container.Resolve<IBallSpawner>(), container.Resolve<IAdService>(), container.Resolve<IPauseService>());

            if (_gameOverUI != null)
            {
                _gameOverUI.Inject(container.Resolve<IPauseService>());
                levelManager.OnGameOver += _gameOverUI.Show;
                _gameOverUI.OnRestart   += levelManager.RestartFromLevel1;
                _gameOverUI.OnWatchAd   += levelManager.ContinueAfterAd;
            }

            _gameStartController.Inject(container.Resolve<IAdService>(), container.Resolve<IGameState>());
            _gameStartController.RegisterInit(_gameGrid.Initialize);
            _gameStartController.RegisterInit(container.Resolve<HippoController>().Initialize);
            if (_hippoRespawnHandler    != null) _gameStartController.RegisterInit(_hippoRespawnHandler.Initialize);
            if (_levelColorController   != null)
            {
                _levelColorController.Inject(container.Resolve<IGameState>());
                _gameStartController.RegisterInit(_levelColorController.Initialize);
            }
            _gameStartController.RegisterInit(container.Resolve<HippoGridInteractor>().Initialize);
            _gameStartController.RegisterInit(levelManager.Initialize);
            if (_livesUI   != null) _gameStartController.RegisterInit(_livesUI.Initialize);
            if (_scoreUI   != null) _gameStartController.RegisterInit(_scoreUI.Initialize);
            if (_levelUI   != null) _gameStartController.RegisterInit(_levelUI.Initialize);
            if (_percentUI != null) _gameStartController.RegisterInit(_percentUI.Initialize);
        }

    }
}
