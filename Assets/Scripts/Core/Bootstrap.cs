using UnityEngine;
using HippoGame.Zone;
using HippoGame.Grid;
using HippoGame.Hippo;
using HippoGame.Ball;
using HippoGame.UI;
using HippoGame.FX;
using HippoGame.Ads;

namespace HippoGame.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private LevelConfig _levelConfig;

        [Header("Systems")]
        [SerializeField] private GameZone               _gameZone;
        [SerializeField] private HippoController        _hippoController;
        [SerializeField] private GameGrid               _gameGrid;
        [SerializeField] private HippoGridInteractor    _hippoGridInteractor;
        [SerializeField] private ParticleEffectsService _particleEffects;
        [SerializeField] private BallSpawner            _ballSpawner;
        [SerializeField] private AdController           _adController;
        [SerializeField] private GameStartController    _gameStartController;
        [SerializeField] private GameStartAdHandler     _gameStartAdHandler;
        [SerializeField] private HippoRespawnHandler    _hippoRespawnHandler;
        [SerializeField] private LevelColorController   _levelColorController;

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
                _levelConfig, _gameZone, _hippoController, _gameGrid,
                _hippoGridInteractor, _particleEffects, _ballSpawner, _adController);

            GameInjector.Inject(
                container,
                _hippoController, _hippoGridInteractor, _ballSpawner,
                _hippoRespawnHandler, _levelColorController,
                _livesUI, _scoreUI, _levelUI, _percentUI,
                _gameOverUI, _shopUI,
                _gameStartController, _gameStartAdHandler);
        }
    }
}
