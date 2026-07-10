using UnityEngine;
using AgavePuzzle.Gameplay;

namespace AgavePuzzle.Audio
{
    /// Central place for playing game sounds. Listens to gameplay events
    /// so gameplay code itself stays audio-agnostic.
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private PuzzleBoard puzzleBoard;

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Clips")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField] private AudioClip connectClip;

        private void OnEnable()
        {
            PieceDragHandler.OnDragStarted += PlayPickup;
            puzzleBoard.OnConnectionMade += HandleConnectionMade;
        }

        private void OnDisable()
        {
            PieceDragHandler.OnDragStarted -= PlayPickup;
            puzzleBoard.OnConnectionMade -= HandleConnectionMade;
        }

        private void Start()
        {
            if (backgroundMusic == null)
            {
                return;
            }

            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        private void PlayPickup()
        {
            if (pickupClip != null)
            {
                sfxSource.PlayOneShot(pickupClip);
            }
        }

        private void HandleConnectionMade(Vector2 _)
        {
            if (connectClip != null)
            {
                sfxSource.PlayOneShot(connectClip);
            }
        }
    }
}