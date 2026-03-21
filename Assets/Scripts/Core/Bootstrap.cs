using UnityEngine;
using HippoGame.Interfaces;
using HippoGame.Zone;
using HippoGame.Input;
using HippoGame.Movement;
using HippoGame.Grid;
using HippoGame.Trail;
using HippoGame.Hippo;
using HippoGame.Ball;
using HippoGame.UI;

namespace HippoGame.Core
{
    /// Единая точка сборки: создаёт все объекты и соединяет зависимости.
    public class Bootstrap : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private GameManager        _gameManager;
        [SerializeField] private GameZone           _gameZone;
        [SerializeField] private HippoController    _hippoController;
        [SerializeField] private GameGrid           _gameGrid;
        [SerializeField] private HippoGridInteractor _hippoGridInteractor;
        [SerializeField] private TrailLineRenderer  _trailLineRenderer;
        [SerializeField] private BallSpawner        _ballSpawner;

        [Header("UI")]
        [SerializeField] private LivesUIController       _livesUI;
        [SerializeField] private ScoreUIController       _scoreUI;
        [SerializeField] private LevelUIController       _levelUI;
        [SerializeField] private FillPercentUIController _percentUI;
        [SerializeField] private GameOverUIController    _gameOverUI;

        private void Awake()
        {
            Debug.Log("[Bootstrap] Awake — сборка зависимостей");
            _gameManager.Register();

            var container = BuildContainer();
            Inject(container);
            InitializeAll(container);
            Debug.Log("[Bootstrap] Готово — игра запущена");
        }

        private DiContainer BuildContainer()
        {
            var container = new DiContainer();
            var gameState = new GameState();
            var movement  = new AutoDirectionalMovement();

            container.Register<GameState>(gameState);
            container.Register<IBoundaryService>(_gameZone);
            container.Register<IInputProvider>(new KeyboardInputProvider());
            container.Register<AutoDirectionalMovement>(movement);
            container.Register<IMovementBehaviour>(movement);
            container.Register<HippoController>(_hippoController);
            container.Register<IGridService>(_gameGrid);
            container.Register<IGridRenderer>(_gameGrid);
            container.Register<IFillService>(new ZoneFillService());
            container.Register<ICollisionService>(new DrawingAwareCollisionService(
                _hippoGridInteractor,
                container.Resolve<IGridService>()
            ));
            container.Register<ITrailService>(new TrailTracker());
            container.Register<ITrailVisualizer>(_trailLineRenderer);
            container.Register<IBallSpawner>(_ballSpawner);
            container.Register<IBallInteractable>(_hippoGridInteractor);
            container.Register<HippoGridInteractor>(_hippoGridInteractor);
            container.Register<LevelManager>(new LevelManager());

            Debug.Log("[Bootstrap] DiContainer собран");
            return container;
        }

        private void Inject(DiContainer container)
        {
            var state = container.Resolve<GameState>();
            var grid  = container.Resolve<IGridService>();

            _trailLineRenderer.Inject(grid);

            _hippoController.Inject(
                container.Resolve<IInputProvider>(),
                container.Resolve<IMovementBehaviour>(),
                container.Resolve<IBoundaryService>(),
                container.Resolve<ICollisionService>()
            );

            _hippoGridInteractor.Inject(
                grid,
                container.Resolve<IFillService>(),
                container.Resolve<ITrailService>(),
                container.Resolve<ITrailVisualizer>(),
                _hippoController.transform,
                state,
                container.Resolve<AutoDirectionalMovement>()
            );

            if (_ballSpawner != null)
                _ballSpawner.Inject(
                    grid,
                    container.Resolve<IBoundaryService>(),
                    _hippoController.transform,
                    container.Resolve<IBallInteractable>()
                );

            container.Resolve<LevelManager>().Inject(
                state,
                grid,
                container.Resolve<IBallSpawner>(),
                _hippoController,
                _hippoGridInteractor,
                container.Resolve<IMovementBehaviour>(),
                container.Resolve<IBoundaryService>()
            );

            if (_livesUI   != null) _livesUI.Inject(state);
            if (_scoreUI   != null) _scoreUI.Inject(state);
            if (_levelUI   != null) _levelUI.Inject(state);
            if (_percentUI != null) _percentUI.Inject(state, grid);

            var levelManager = container.Resolve<LevelManager>();
            if (_gameOverUI != null)
            {
                levelManager.OnGameOver += _gameOverUI.Show;
                _gameOverUI.OnRestart   += levelManager.RestartFromLevel1;
                _gameOverUI.OnWatchAd   += levelManager.ContinueAfterAd;
            }

            Debug.Log("[Bootstrap] Inject завершён");
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
