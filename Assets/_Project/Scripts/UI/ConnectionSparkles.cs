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
            Vector3 worldPosition = boardContainer.TransformPoint(boardPosition);

            worldPosition.z -= 0.5f;

            burstInstance.transform.position = worldPosition;
            burstInstance.Play();
        }
    }
}