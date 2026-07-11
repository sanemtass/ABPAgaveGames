using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AgavePuzzle.Gameplay
{
    /// Handles pointer drag for a piece. Dragging always moves the piece's
    /// whole connected group; the drop result is decided by PuzzleBoard.
    [RequireComponent(typeof(PuzzlePiece))]
    [RequireComponent(typeof(CanvasGroup))]
    public class PieceDragHandler : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static event Action<PieceDragHandler, GridCoordinate> OnGroupDropped;
        public static event Action OnDragStarted;

        [Header("Return Animation")]
        [SerializeField] private float returnDuration = 0.25f;

        [Header("Lift Animation")]
        [SerializeField] private float liftScale = 1.06f;
        [SerializeField] private float liftDuration = 0.12f;

        private PuzzlePiece piece;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private RectTransform boardRectTransform;
        private ConnectionEvaluator connectionEvaluator;

        private readonly List<PuzzlePiece> draggedGroup = new List<PuzzlePiece>();
        private readonly List<Vector2> groupStartPositions = new List<Vector2>();

        private Vector2 pointerStartLocalPoint;

        public PuzzlePiece Piece => piece;
        public IReadOnlyList<PuzzlePiece> DraggedGroup => draggedGroup;

        private void Awake()
        {
            piece = GetComponent<PuzzlePiece>();
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = piece.RectTransform;
        }

        public void Configure(RectTransform boardRect, ConnectionEvaluator evaluator)
        {
            boardRectTransform = boardRect;
            connectionEvaluator = evaluator;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnDragStarted?.Invoke();

            draggedGroup.Clear();
            groupStartPositions.Clear();

            IReadOnlyList<PuzzlePiece> connectedGroup =
                connectionEvaluator.GetConnectedGroup(piece.CurrentCoordinate);
            draggedGroup.AddRange(connectedGroup);

            foreach (PuzzlePiece groupPiece in draggedGroup)
            {
                RectTransform pieceRect = groupPiece.RectTransform;

                // DOKill leaves a half-finished tween wherever it was, so reset
                // scale explicitly or rapid re-drags accumulate a wrong size.
                pieceRect.DOKill();
                pieceRect.localScale = Vector3.one;

                groupStartPositions.Add(pieceRect.anchoredPosition);
                pieceRect.SetAsLastSibling();

                pieceRect.DOScale(liftScale, liftDuration)
                    .SetEase(Ease.OutQuad)
                    .SetLink(pieceRect.gameObject);
            }
            
            canvasGroup.blocksRaycasts = false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                boardRectTransform, eventData.position, eventData.pressEventCamera,
                out pointerStartLocalPoint);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    boardRectTransform, eventData.position, eventData.pressEventCamera,
                    out Vector2 currentLocalPoint))
            {
                return;
            }

            Vector2 dragDelta = currentLocalPoint - pointerStartLocalPoint;

            for (int i = 0; i < draggedGroup.Count; i++)
            {
                draggedGroup[i].RectTransform.anchoredPosition = groupStartPositions[i] + dragDelta;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;

            foreach (PuzzlePiece groupPiece in draggedGroup)
            {
                groupPiece.RectTransform.DOScale(1f, liftDuration)
                    .SetEase(Ease.OutQuad)
                    .SetLink(groupPiece.RectTransform.gameObject);
            }

            GridCoordinate targetCoordinate = CalculateTargetCoordinate();
            OnGroupDropped?.Invoke(this, targetCoordinate);
        }

        public void ReturnToStart()
        {
            for (int i = 0; i < draggedGroup.Count; i++)
            {
                RectTransform pieceRect = draggedGroup[i].RectTransform;

                // Same reset as in OnBeginDrag: killing the shrink tween mid-way
                // would otherwise strand the piece at a partial scale.
                pieceRect.DOKill();
                pieceRect.localScale = Vector3.one;

                pieceRect.DOAnchorPos(groupStartPositions[i], returnDuration)
                    .SetEase(Ease.OutBack)
                    .SetLink(pieceRect.gameObject);
            }
        }

        /// Maps the dragged piece's center to a grid cell. Assumes a
        /// middle-center pivot/anchor on the piece (set on the prefab).
        private GridCoordinate CalculateTargetCoordinate()
        {
            float boardWidth = boardRectTransform.rect.width;
            float boardHeight = boardRectTransform.rect.height;
            float cellWidth = rectTransform.rect.width;
            float cellHeight = rectTransform.rect.height;

            Vector2 position = rectTransform.anchoredPosition;

            float normalizedX = position.x + boardWidth / 2f;
            float normalizedY = boardHeight / 2f - position.y;

            int column = Mathf.FloorToInt(normalizedX / cellWidth);
            int row = Mathf.FloorToInt(normalizedY / cellHeight);

            return new GridCoordinate(row, column);
        }
    }
}