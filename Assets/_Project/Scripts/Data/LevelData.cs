using UnityEngine;

namespace AgavePuzzle.Data
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Agave/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int levelNumber = 1;

        [Header("Visuals")]
        [SerializeField] private Color gameBackgroundColor = new Color(1f, 0.941f, 0.859f); // #FFF0DB
        [SerializeField] private Color frameColor = new Color(0.169f, 0.169f, 0.169f);       // #2B2B2B
        [SerializeField] private Texture2D sourceImage;

        [Header("Grid Settings")]
        [SerializeField] [Range(2, 8)] private int gridWidth = 3;
        [SerializeField] [Range(2, 8)] private int gridHeight = 4;

        [Header("Gameplay")]
        [SerializeField] private int moveCount = 15;

        public int LevelNumber => levelNumber;
        public Color GameBackgroundColor => gameBackgroundColor;
        public Color FrameColor => frameColor;
        public Texture2D SourceImage => sourceImage;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public int MoveCount => moveCount;
    }
}