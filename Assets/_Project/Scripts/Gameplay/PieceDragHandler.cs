using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AgavePuzzle.Gameplay
{
    [RequireComponent(typeof(PuzzlePiece))]
    [RequireComponent(typeof(CanvasGroup))]
    public class PieceDragHandler : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static event Action<PieceDragHandler, GridCoordinate> OnGroupDropped;

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
            draggedGroup.Clear();
            groupStartPositions.Clear();

            IReadOnlyList<PuzzlePiece> connectedGroup =
                connectionEvaluator.GetConnectedGroup(piece.CurrentCoordinate);
            draggedGroup.AddRange(connectedGroup);

            foreach (PuzzlePiece groupPiece in draggedGroup)
            {
                groupStartPositions.Add(groupPiece.RectTransform.anchoredPosition);
                groupPiece.RectTransform.SetAsLastSibling();
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

            GridCoordinate targetCoordinate = CalculateTargetCoordinate();
            OnGroupDropped?.Invoke(this, targetCoordinate);
        }

        public void ReturnToStart()
        {
            for (int i = 0; i < draggedGroup.Count; i++)
            {
                draggedGroup[i].RectTransform.anchoredPosition = groupStartPositions[i];
            }
        }

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