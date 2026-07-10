using UnityEngine;
using AgavePuzzle.Gameplay;

namespace AgavePuzzle.UI
{
    /// Plays a star-burst ParticleSystem when a swap creates a new connection.
    /// The canvas runs in Screen Space - Camera mode so world-space particles
    /// can sort in front of the UI via sorting order.
    public class ConnectionSparkles : MonoBehaviour
    {
        [SerializeField] private PuzzleBoard puzzleBoard;
        [SerializeField] private ParticleSystem burstPrefab;
        [SerializeField] private RectTransform boardContainer;

        private ParticleSystem burstInstance;

        private void Awake()
        {
            // Single reusable instance: bursts are short and infrequent,
            // so one system is enough (no pool of systems needed).
            burstInstance = Instantiate(burstPrefab);
        }

        private void OnEnable()
        {
            puzzleBoard.OnConnectionMade += PlayBurst;
        }

        private void OnDisable()
        {
            puzzleBoard.OnConnectionMade -= PlayBurst;
        }

        private void PlayBurst(Vector2 boardPosition)
        {
            // GetCellPosition gives a point in boardContainer's local space;
            // TransformPoint converts it to world space for the particle system.
            Vector3 worldPosition = boardContainer.TransformPoint(boardPosition);

            // Nudge toward the camera so the burst spawns in front of the canvas plane.
            worldPosition.z -= 0.5f;

            burstInstance.transform.position = worldPosition;
            burstInstance.Play();
        }
    }
}