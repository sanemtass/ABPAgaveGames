using UnityEngine;

namespace AgavePuzzle.Data
{
    [CreateAssetMenu(fileName = "LevelSequence", menuName = "Agave/Level Sequence")]
    public class LevelSequence : ScriptableObject
    {
        [SerializeField] private LevelData[] levels;

        public int LevelCount => levels.Length;
        public LevelData GetLevel(int index) => levels[index];
        public bool HasNextLevel(int index) => index + 1 < levels.Length;
    }
}