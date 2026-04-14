using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HippoGame.Core;
using HippoGame.Ball;
using HippoGame.Hippo;

namespace HippoGame.Audio
{
    /// Единственный скрипт для всех звуков.
    ///
    /// ПРАВИЛА:
    ///  - Кнопки и отскок арбуза играют ВСЕГДА, независимо от состояния музыки.
    ///  - Когда играет звук начала уровня или проигрыша — музыка ВЫКЛЮЧАЕТСЯ.
    ///  - После звука начала уровня — музыка возобновляется.
    ///  - После звука проигрыша — музыка остаётся выключенной.
    public class AudioService : MonoBehaviour
    {
        [Serializable]
        private struct Sound
        {
            public AudioClip          clip;
            [Range(0f, 1f)] public float volume;
        }

        public static AudioService Instance { get; private set; }

        [Header("Music")]
        [Tooltip("AudioSource для фоновой музыки (loop)")]
        [SerializeField] private AudioSource _musicSource;
        [Tooltip("Основной фоновый трек — играет всё время")]
        [SerializeField] private Sound _bgMusic = new Sound { volume = 1f };

        [Header("SFX")]
        [Tooltip("AudioSource для коротких звуков (PlayOneShot)")]
        [SerializeField] private AudioSource _sfxSource;
        [Tooltip("Начало уровня — музыка выключается на время звука, потом возобновляется")]
        [SerializeField] private Sound _levelStartSound  = new Sound { volume = 1f };
        [Tooltip("Арбуз попал в бегемота или трейл — потеря жизни")]
        [SerializeField] private Sound _hitLifeSound     = new Sound { volume = 1f };
        [Tooltip("Кончились все жизни — музыка после не возобновляется")]
        [SerializeField] private Sound _gameOverSound    = new Sound { volume = 1f };
        [Tooltip("Бегемот закрасил кусок поля и вернулся на стену")]
        [SerializeField] private Sound _zoneFillSound    = new Sound { volume = 1f };
        [Tooltip("Отскок арбуза от стены или закрашенной зоны")]
        [SerializeField] private Sound _bounceSound      = new Sound { volume = 1f };
        [Tooltip("Нажатие любой кнопки UI")]
        [SerializeField] private Sound _buttonClickSound = new Sound { volume = 1f };

        [Header("Buttons — перетащи все кнопки сюда")]
        [Tooltip("Все кнопки игры — при нажатии сыграет Button Click Sound")]
        [SerializeField] private Button[] _buttons;

        // ── Awake ────────────────────────────────────────────────────────────
        private void Awake()
        {
            Instance = this;
            foreach (var btn in _buttons)
                if (btn != null)
                    btn.onClick.AddListener(PlayButtonClick);
        }

        // ── Инициализация ────────────────────────────────────────────────────
        public void Initialize(LevelManager        levelManager,
                               BallSpawner         ballSpawner,
                               HippoGridInteractor hippoGridInteractor)
        {
            levelManager.OnLevelStarted      += OnLevelStarted;
            levelManager.OnGameOver          += OnGameOver;
            ballSpawner.OnAnyBallBounce      += PlayBounce;
            hippoGridInteractor.OnHit        += PlayHitLife;
            hippoGridInteractor.OnZoneFilled += PlayZoneFill;

            StartMusic();
            Debug.Log("[AudioService] Инициализирован");
        }

        // ── Музыка ───────────────────────────────────────────────────────────
        private void StartMusic()
        {
            if (_musicSource == null || _bgMusic.clip == null) return;
            _musicSource.clip   = _bgMusic.clip;
            _musicSource.volume = _bgMusic.volume;
            _musicSource.loop   = true;
            _musicSource.Play();
        }

        private void StopMusic()   => _musicSource?.Stop();
        private void ResumeMusic() => _musicSource?.Play();

        // ── Игровые события ──────────────────────────────────────────────────
        private void OnLevelStarted()
        {
            if (_levelStartSound.clip == null) return;
            StopMusic();
            _sfxSource.PlayOneShot(_levelStartSound.clip, _levelStartSound.volume);
            StartCoroutine(ResumeAfter(_levelStartSound.clip.length));
        }

        private void OnGameOver()
        {
            StopMusic();
            PlaySfx(_gameOverSound);
        }

        // ── Всегда играют ────────────────────────────────────────────────────
        private void PlayZoneFill()        => PlaySfx(_zoneFillSound);
        private void PlayBounce()          => PlaySfx(_bounceSound);
        private void PlayHitLife()         => PlaySfx(_hitLifeSound);
        public  void PlayButtonClick()     => PlaySfx(_buttonClickSound);

        // ── Вспомогательные ──────────────────────────────────────────────────
        private void PlaySfx(Sound sound)
        {
            if (sound.clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(sound.clip, sound.volume);
        }

        private IEnumerator ResumeAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResumeMusic();
        }
    }
}
