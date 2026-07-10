using UnityEngine;
using UnityEngine.UI;

namespace AgavePuzzle.Gameplay
{
    [RequireComponent(typeof(RectTransform))]
    public class PuzzlePiece : MonoBehaviour
    {
        [Header("Visual References")]
        [SerializeField] private Image pieceImage;
        [SerializeField] private Image borderTop;
        [SerializeField] private Image borderRight;
        [SerializeField] private Image borderBottom;
        [SerializeField] private Image borderLeft;

        private RectTransform rectTransform;

        public GridCoordinate CorrectCoordinate { get; private set; }
        public GridCoordinate CurrentCoordinate { get; private set; }
        public bool IsInCorrectPosition => CurrentCoordinate == CorrectCoordinate;
        public RectTransform RectTransform => rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(GridCoordinate correctCoordinate, Sprite pieceSprite, Color frameColor)
        {
            CorrectCoordinate = correctCoordinate;
            pieceImage.sprite = pieceSprite;

            borderTop.color = frameColor;
            borderRight.color = frameColor;
            borderBottom.color = frameColor;
            borderLeft.color = frameColor;
        }

        public void SetCurrentCoordinate(GridCoordinate newCoordinate)
        {
            CurrentCoordinate = newCoordinate;
        }

        public void SetBorderVisibility(bool top, bool right, bool bottom, bool left)
        {
            borderTop.enabled = top;
            borderRight.enabled = right;
            borderBottom.enabled = bottom;
            borderLeft.enabled = left;
        }
    }
}