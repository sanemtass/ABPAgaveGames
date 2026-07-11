using UnityEngine;
using AgavePuzzle.Gameplay;

namespace AgavePuzzle.Audio
{
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
        [SerializeField] private AudioClip loseClip;

        private void OnEnable()
        {
            PieceDragHandler.OnDragStarted += PlayPickup;
            puzzleBoard.OnConnectionMade += HandleConnectionMade;
            puzzleBoard.OnLevelLost += PlayLose;
        }

        private void OnDisable()
        {
            PieceDragHandler.OnDragStarted -= PlayPickup;
            puzzleBoard.OnConnectionMade -= HandleConnectionMade;
            puzzleBoard.OnLevelLost -= PlayLose;
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
        
        private void PlayLose()
        {
            if (loseClip != null)
            {
                sfxSource.PlayOneShot(loseClip);
            }
        }
    }
}