using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using AgavePuzzle.Gameplay;

namespace AgavePuzzle.UI
{
    /// Shows the tutorial hand demonstrating a connecting swap on the board.
    /// Hides permanently after the player makes their first move.
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Image))]
    public class TutorialHandController : MonoBehaviour
    {
        [SerializeField] private PuzzleBoard puzzleBoard;

        [Header("Sizing")]
        [SerializeField] private float handSizeMultiplier = 1.3f;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private float pressScale = 0.85f;
        [SerializeField] private float pressDuration = 0.15f;
        [SerializeField] private float moveDuration = 0.8f;
        [SerializeField] private float loopPause = 0.5f;

        private RectTransform handRect;
        private CanvasGroup handCanvasGroup;
        private Image handImage;
        private Sequence handSequence;

        private void Awake()
        {
            handRect = (RectTransform)transform;
            handCanvasGroup = GetComponent<CanvasGroup>();
            handImage = GetComponent<Image>();
        }

        private void OnEnable()
        {
            puzzleBoard.OnBoardInitialized += HandleBoardInitialized;
        }

        private void OnDisable()
        {
            puzzleBoard.OnBoardInitialized -= HandleBoardInitialized;

            if (puzzleBoard.MoveCounter != null)
            {
                puzzleBoard.MoveCounter.OnMovesChanged -= HandleFirstMove;
            }

            handSequence?.Kill();
        }

        private void HandleBoardInitialized()
        {
            puzzleBoard.MoveCounter.OnMovesChanged += HandleFirstMove;
            handRect.SetAsLastSibling();

            Vector2 cellSize = puzzleBoard.GetCellSize();
            float referenceSize = Mathf.Min(cellSize.x, cellSize.y);
            float targetWidth = referenceSize * handSizeMultiplier;
            float spriteAspect = handImage.sprite.rect.width / handImage.sprite.rect.height;
            handRect.sizeDelta = new Vector2(targetWidth, targetWidth / spriteAspect);

            var swapFinder = new TutorialSwapFinder(
                puzzleBoard.BoardState, puzzleBoard.ConnectionEvaluator);

            if (swapFinder.TryFindConnectingSwap(out GridCoordinate from, out GridCoordinate to))
            {
                PlayDemo(puzzleBoard.GetCellPosition(from), puzzleBoard.GetCellPosition(to));
            }
            else
            {
                handRect.gameObject.SetActive(false);
            }
        }

        private void HandleFirstMove(int movesRemaining)
        {
            puzzleBoard.MoveCounter.OnMovesChanged -= HandleFirstMove;
            handSequence?.Kill();
            handRect.gameObject.SetActive(false);
        }

        private void PlayDemo(Vector2 fromPosition, Vector2 toPosition)
        {
            handCanvasGroup.alpha = 0f;

            // Built once, looped forever; killed on first move / disable (skill rule: reuse sequences).
            handSequence = DOTween.Sequence()
                .AppendCallback(() => handRect.anchoredPosition = fromPosition)
                .Append(handCanvasGroup.DOFade(1f, fadeDuration))
                .Append(handRect.DOScale(pressScale, pressDuration))
                .Append(handRect.DOAnchorPos(toPosition, moveDuration).SetEase(Ease.InOutQuad))
                .Append(handRect.DOScale(1f, pressDuration))
                .Append(handCanvasGroup.DOFade(0f, fadeDuration))
                .AppendInterval(loopPause)
                .SetLoops(-1)
                .SetLink(gameObject);
        }
    }
}