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

namespace HippoGame.Core
{
    /// Единая точка сборки: создаёт все объекты и соединяет зависимости.
    public class Bootstrap : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private LevelConfig               _levelConfig;

        [Header("Systems")]
        [SerializeField] private GameZone                  _gameZone;
        [SerializeField] private HippoController           _hippoController;
        [SerializeField] private GameGrid                  _gameGrid;
        [SerializeField] private HippoGridInteractor       _hippoGridInteractor;
        [SerializeField] private ParticleEffectsService    _particleEffects;
        [SerializeField] private BallSpawner               _ballSpawner;

        [Header("UI")]
        [SerializeField] private LivesUIController       _livesUI;
        [SerializeField] private ScoreUIController       _scoreUI;
        [SerializeField] private LevelUIController       _levelUI;
        [SerializeField] private FillPercentUIController _percentUI;
        [SerializeField] private GameOverUIController    _gameOverUI;
        [SerializeField] private ShopUIController       _shopUI;

        private void Awake()
        {
            Debug.Log("[Bootstrap] Awake — сборка зависимостей");
            var container = BuildContainer();
            Inject(container);
            InitializeAll(container);
            Debug.Log("[Bootstrap] Готово — игра запущена");
        }

        private DiContainer BuildContainer()
        {
            var container = new DiContainer();
            var gameState = new GameState(_levelConfig);
            container.Register<GameState>(gameState);
            container.Register<IGameState>(gameState);
            container.Register<IBoundaryService>(_gameZone);
            container.Register<IInputProvider>(new KeyboardInputProvider());
            container.Register<HippoController>(_hippoController);
            container.Register<IGridService>(_gameGrid);
            container.Register<IGridRenderer>(_gameGrid);
            var movement = new CellMovement(container.Resolve<IGridService>());
            container.Register<CellMovement>(movement);
            container.Register<IMovementBehaviour>(movement);
            container.Register<IFillService>(new FloodFillService());
            var grid = container.Resolve<IGridService>();
            container.Register<ICollisionService>(new DrawingAwareCollisionService(
                _hippoGridInteractor,
                grid,
                new CellCollisionService(grid)
            ));
            container.Register<IHippoController>(_hippoController);
            container.Register<IHippoGridInteractor>(_hippoGridInteractor);
            container.Register<ITrailService>(new TrailTracker());
            container.Register<IBallSpawner>(_ballSpawner);
            container.Register<IBallInteractable>(_hippoGridInteractor);
            if (_particleEffects != null)
                container.Register<IParticleService>(_particleEffects);
            container.Register<HippoGridInteractor>(_hippoGridInteractor);
            container.Register<LevelManager>(new LevelManager());

            Debug.Log("[Bootstrap] DiContainer собран");
            return container;
        }

        private void Inject(DiContainer container)
        {
            var state = container.Resolve<GameState>();
            var grid  = container.Resolve<IGridService>();

            _hippoController.Inject(
                container.Resolve<IInputProvider>(),
                container.Resolve<IMovementBehaviour>(),
                container.Resolve<IBoundaryService>(),
                container.Resolve<ICollisionService>(),
                container.Resolve<IGridService>(),
                _hippoGridInteractor
            );

            _hippoGridInteractor.Inject(
                grid,
                container.Resolve<IFillService>(),
                container.Resolve<ITrailService>(),
                _hippoController.transform,
                container.Resolve<IMovementBehaviour>(),
                _particleEffects != null ? container.Resolve<IParticleService>() : null,
                container.Resolve<IBallSpawner>()
            );

            _hippoGridInteractor.OnHit += () =>
            {
                container.Resolve<IGameState>().LoseLife();
                Debug.Log("[TRAIL CLEAR] причина: Bootstrap.OnHit → LoseLife");
            };

            if (_ballSpawner != null)
                _ballSpawner.Inject(
                    grid,
                    container.Resolve<IBoundaryService>(),
                    _hippoController.transform,
                    container.Resolve<IBallInteractable>()
                );

            container.Resolve<LevelManager>().Inject(
                container.Resolve<IGameState>(),
                grid,
                container.Resolve<IBallSpawner>(),
                container.Resolve<IHippoController>(),
                container.Resolve<IHippoGridInteractor>(),
                container.Resolve<IMovementBehaviour>(),
                container.Resolve<IBoundaryService>()
            );

            if (_livesUI   != null) _livesUI.Inject(state);
            if (_scoreUI   != null) _scoreUI.Inject(state);
            if (_levelUI   != null) _levelUI.Inject(state);
            if (_percentUI != null) _percentUI.Inject(state, grid);

            if (_shopUI != null)
                _shopUI.Inject(container.Resolve<IGameState>(), container.Resolve<IBallSpawner>());

            var levelManager = container.Resolve<LevelManager>();
            if (_gameOverUI != null)
            {
                levelManager.OnGameOver += _gameOverUI.Show;
                _gameOverUI.OnRestart   += levelManager.RestartFromLevel1;
                _gameOverUI.OnWatchAd   += levelManager.ContinueAfterAd;
            }

            Debug.Log("[Bootstrap] Inject завершён");
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }

        private void InitializeAll(DiContainer container)
        {
            container.Resolve<HippoController>().Initialize();
            container.Resolve<HippoGridInteractor>().Initialize();
            container.Resolve<LevelManager>().Initialize();

            if (_livesUI   != null) _livesUI.Initialize();
            if (_scoreUI   != null) _scoreUI.Initialize();
            if (_levelUI   != null) _levelUI.Initialize();
            if (_percentUI != null) _percentUI.Initialize();

            Debug.Log("[Bootstrap] InitializeAll завершён");
        }
    }
}
