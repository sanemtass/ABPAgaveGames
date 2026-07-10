using UnityEngine;

namespace AgavePuzzle.Core
{
    /// Applies application-level settings once at startup.
    public class GameSettings : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
        }
    }
}