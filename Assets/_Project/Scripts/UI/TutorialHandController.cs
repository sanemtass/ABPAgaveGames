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
        [SerializeField] private float handSizeMultiplier = 1.1f;
        [SerializeField] private float minHandWidth = 70f;
        [SerializeField] private float maxHandWidth = 170f;

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

            ApplyHandSize();

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

        private void ApplyHandSize()
        {
            Vector2 cellSize = puzzleBoard.GetCellSize();
            float referenceSize = Mathf.Min(cellSize.x, cellSize.y);
            float targetWidth = Mathf.Clamp(
                referenceSize * handSizeMultiplier, minHandWidth, maxHandWidth);

            float spriteAspect = handImage.sprite.rect.width / handImage.sprite.rect.height;
            handRect.sizeDelta = new Vector2(targetWidth, targetWidth / spriteAspect);
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

            handSequence = DOTween.Sequence()
                .AppendCallback(() =>
                {
                    handRect.SetAsLastSibling();
                    handRect.anchoredPosition = fromPosition;
                })
                .Append(handCanvasGroup.DOFade(1f, fadeDuration))
                .Append(handRect.DOScale(new Vector3(1.08f, pressScale, 1f), pressDuration))
                .Append(handRect.DOAnchorPos(toPosition, moveDuration).SetEase(Ease.InOutQuad))
                .Append(handRect.DOScale(Vector3.one, pressDuration).SetEase(Ease.OutBack))
                .Append(handCanvasGroup.DOFade(0f, fadeDuration))
                .AppendInterval(loopPause)
                .SetLoops(-1)
                .SetLink(gameObject);
        }
    }
}