using UnityEngine;

namespace HippoGame.Audio
{
    public class MusicSwitcher : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource _audioSource;

        [Header("Tracks")]
        [SerializeField] private AudioClip _track1;
        [SerializeField] private AudioClip _track2;
        [SerializeField] private AudioClip _track3;
        [SerializeField] private AudioClip _track4;
        [SerializeField] private AudioClip _track5;

        private AudioClip[] _tracks;
        private int         _currentIndex = -1;

        private void Awake()
        {
            _tracks = new[] { _track1, _track2, _track3, _track4, _track5 };
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1)) PlayTrack(0);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2)) PlayTrack(1);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3)) PlayTrack(2);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4)) PlayTrack(3);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5)) PlayTrack(4);
        }

        private void PlayTrack(int index)
        {
            if (index == _currentIndex) return;

            AudioClip clip = _tracks[index];
            if (clip == null)
            {
                Debug.LogWarning($"[MusicSwitcher] Трек {index + 1} не назначен");
                return;
            }

            _currentIndex = index;
            _audioSource.clip = clip;
            _audioSource.loop = true;
            _audioSource.Play();

            Debug.Log($"[MusicSwitcher] Играет трек {index + 1}: {clip.name}");
        }
    }
}
