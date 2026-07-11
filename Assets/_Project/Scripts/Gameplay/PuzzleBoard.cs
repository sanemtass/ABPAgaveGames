using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using AgavePuzzle.Data;

namespace AgavePuzzle.Gameplay
{
    /// Owns the board lifecycle: builds the level, wires the plain C# logic
    /// services together, and reacts to drops reported by PieceDragHandler.
    public class PuzzleBoard : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] private LevelSequence levelSequence;

        [Header("Board References")]
        [SerializeField] private RectTransform boardContainer;
        [SerializeField] private RectTransform celebrationRoot;
        [SerializeField] private PuzzlePiece piecePrefab;
        [SerializeField] private Image backgroundImage;

        [Header("Animation")]
        [SerializeField] private float snapDuration = 0.2f;
        [SerializeField] private float winCelebrationDuration = 1.5f;

        // The case requires the puzzle area to be 9:16 regardless of the
        // source image's own aspect ratio; SlicePuzzleImage crops to this.
        private const float BoardAspectRatio = 9f / 16f;

        private LevelData levelData;
        private int gridWidth;
        private int gridHeight;
        private BoardState boardState;
        private ConnectionEvaluator connectionEvaluator;
        private GroupSwapService groupSwapService;
        private MoveCounter moveCounter;
        private bool isGameOver;
        private readonly List<Sprite> createdSprites = new List<Sprite>();

        public BoardState BoardState => boardState;
        public ConnectionEvaluator ConnectionEvaluator => connectionEvaluator;
        public MoveCounter MoveCounter => moveCounter;
        public bool HasNextLevel => levelSequence.HasNextLevel(LevelProgress.CurrentLevelIndex);

        public event Action OnLevelWon;
        public event Action OnLevelLost;
        public event Action OnBoardInitialized;
        public event Action<Vector2> OnConnectionMade;

        private void OnEnable()
        {
            PieceDragHandler.OnGroupDropped -= HandleGroupDropped;
            PieceDragHandler.OnGroupDropped += HandleGroupDropped;
        }

        private void OnDisable()
        {
            PieceDragHandler.OnGroupDropped -= HandleGroupDropped;
        }

        private void Start()
        {
            levelData = levelSequence.GetLevel(LevelProgress.CurrentLevelIndex);
            BuildBoard();
        }

        public Vector2 GetCellSize() =>
            new Vector2(boardContainer.rect.width / gridWidth, boardContainer.rect.height / gridHeight);

        /// Cell center in boardContainer's local space. Anything positioned with
        /// this value must be a child of boardContainer to land correctly.
        public Vector2 GetCellPosition(GridCoordinate coordinate)
        {
            float cellWidth = boardContainer.rect.width / gridWidth;
            float cellHeight = boardContainer.rect.height / gridHeight;

            float posX = coordinate.Column * cellWidth + cellWidth / 2f - boardContainer.rect.width / 2f;
            float posY = boardContainer.rect.height / 2f - (coordinate.Row * cellHeight + cellHeight / 2f);

            return new Vector2(posX, posY);
        }

        private void BuildBoard()
        {
            // The board sizes cells from boardContainer.rect, which is driven by
            // the AspectRatioFitter; force a layout pass so the rect is final
            // before any cell math happens.
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(boardContainer);

            gridWidth = levelData.GridWidth;
            gridHeight = levelData.GridHeight;
            boardState = new BoardState(gridWidth, gridHeight);
            connectionEvaluator = new ConnectionEvaluator(boardState);
            groupSwapService = new GroupSwapService(boardState);
            moveCounter = new MoveCounter(levelData.MoveCount);

            if (backgroundImage != null)
            {
                backgroundImage.color = levelData.GameBackgroundColor;
            }

            List<Sprite> pieceSprites = SlicePuzzleImage(levelData.SourceImage, gridWidth, gridHeight);
            createdSprites.AddRange(pieceSprites);
            List<GridCoordinate> shuffledCoordinates = GenerateShuffledCoordinates(gridWidth, gridHeight);

            float cellWidth = boardContainer.rect.width / gridWidth;
            float cellHeight = boardContainer.rect.height / gridHeight;

            int spriteIndex = 0;
            for (int row = 0; row < gridHeight; row++)
            {
                for (int column = 0; column < gridWidth; column++)
                {
                    GridCoordinate correctCoordinate = new GridCoordinate(row, column);
                    GridCoordinate startingCoordinate = shuffledCoordinates[spriteIndex];

                    PuzzlePiece piece = Instantiate(piecePrefab, boardContainer);
                    piece.Initialize(correctCoordinate, pieceSprites[spriteIndex], levelData.FrameColor);
                    PieceDragHandler dragHandler = piece.GetComponent<PieceDragHandler>();
                    dragHandler.Configure(boardContainer, connectionEvaluator);
                    boardState.PlacePiece(piece, startingCoordinate);
                    PositionPiece(piece, startingCoordinate, cellWidth, cellHeight);

                    spriteIndex++;
                }
            }

            connectionEvaluator.RefreshAllConnections();
            OnBoardInitialized?.Invoke();
        }

        /// Cuts the source texture into grid cells after center-cropping it to
        /// the board's 9:16 ratio, so the image is never stretched or squashed.
        private List<Sprite> SlicePuzzleImage(Texture2D sourceTexture, int columns, int rows)
        {
            var sprites = new List<Sprite>();

            float textureAspect = (float)sourceTexture.width / sourceTexture.height;

            float cropWidth = sourceTexture.width;
            float cropHeight = sourceTexture.height;

            if (textureAspect > BoardAspectRatio)
            {
                cropWidth = sourceTexture.height * BoardAspectRatio;
            }
            else
            {
                cropHeight = sourceTexture.width / BoardAspectRatio;
            }

            float offsetX = (sourceTexture.width - cropWidth) / 2f;
            float offsetY = (sourceTexture.height - cropHeight) / 2f;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    // Row 0 is the top of the board, but texture coordinates start
                    // at the bottom -- hence the inverted Y math.
                    int xMin = Mathf.RoundToInt(offsetX + column * cropWidth / columns);
                    int xMax = Mathf.RoundToInt(offsetX + (column + 1) * cropWidth / columns);

                    int yMax = Mathf.RoundToInt(offsetY + cropHeight - row * cropHeight / rows);
                    int yMin = Mathf.RoundToInt(offsetY + cropHeight - (row + 1) * cropHeight / rows);

                    Rect spriteRect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
                    sprites.Add(Sprite.Create(sourceTexture, spriteRect, new Vector2(0.5f, 0.5f)));
                }
            }

            return sprites;
        }

        private List<GridCoordinate> GenerateShuffledCoordinates(int columns, int rows)
        {
            var coordinates = new List<GridCoordinate>();
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    coordinates.Add(new GridCoordinate(row, column));
                }
            }

            // Reshuffle until at least one piece is out of place; a fully solved
            // start is possible by pure chance (quite likely on small grids like 2x2).
            do
            {
                for (int i = coordinates.Count - 1; i > 0; i--)
                {
                    int randomIndex = UnityEngine.Random.Range(0, i + 1);
                    (coordinates[i], coordinates[randomIndex]) = (coordinates[randomIndex], coordinates[i]);
                }
            }
            while (IsSolvedArrangement(coordinates, columns));

            return coordinates;
        }

        private static bool IsSolvedArrangement(List<GridCoordinate> shuffled, int columns)
        {
            for (int i = 0; i < shuffled.Count; i++)
            {
                GridCoordinate correct = new GridCoordinate(i / columns, i % columns);
                if (shuffled[i] != correct)
                {
                    return false;
                }
            }

            return true;
        }

        private void PositionPiece(PuzzlePiece piece, GridCoordinate coordinate, float cellWidth, float cellHeight)
        {
            RectTransform pieceRect = piece.RectTransform;
            pieceRect.sizeDelta = new Vector2(cellWidth, cellHeight);
            pieceRect.anchoredPosition = GetCellPosition(coordinate);
        }

        private void AnimatePieceToPosition(PuzzlePiece piece, GridCoordinate coordinate)
        {
            Vector2 target = GetCellPosition(coordinate);
            RectTransform pieceRect = piece.RectTransform;

            // Most pieces don't move in a swap; skip them instead of starting
            // tweens that would do nothing.
            if (pieceRect.anchoredPosition == target)
            {
                return;
            }

            pieceRect.DOAnchorPos(target, snapDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(piece.gameObject);
        }

        private void HandleGroupDropped(PieceDragHandler handler, GridCoordinate targetCoordinate)
        {
            if (isGameOver)
            {
                handler.ReturnToStart();
                return;
            }

            GridCoordinate delta = targetCoordinate - handler.Piece.CurrentCoordinate;

            // Zero delta or an invalid swap both return the group without
            // consuming a move (case rule).
            if (delta == new GridCoordinate(0, 0) ||
                !groupSwapService.TryExecuteGroupSwap(handler.DraggedGroup, delta))
            {
                handler.ReturnToStart();
                return;
            }

            int groupSizeBefore = handler.DraggedGroup.Count;

            SyncAllPiecePositions();
            moveCounter.ConsumeMove();
            connectionEvaluator.RefreshAllConnections();

            // If the dragged piece's group grew, this swap created at least one
            // new connection -- fire the event for VFX/SFX listeners.
            int groupSizeAfter =
                connectionEvaluator.GetConnectedGroup(handler.Piece.CurrentCoordinate).Count;
            if (groupSizeAfter > groupSizeBefore)
            {
                OnConnectionMade?.Invoke(GetCellPosition(handler.Piece.CurrentCoordinate));
            }

            CheckGameEnd();
        }

        private void SyncAllPiecePositions()
        {
            foreach (GridCoordinate coordinate in boardState.GetAllCoordinates())
            {
                PuzzlePiece pieceAtCell = boardState.GetPieceAt(coordinate);
                if (pieceAtCell != null)
                {
                    AnimatePieceToPosition(pieceAtCell, coordinate);
                }
            }
        }

        private void CheckGameEnd()
        {
            if (boardState.AreAllPiecesCorrect())
            {
                isGameOver = true;
                PlayWinCelebration();
            }
            else if (moveCounter.MovesRemaining <= 0)
            {
                isGameOver = true;
                PlayLossDelay();
            }
        }

        private void OnDestroy()
        {
            // Sprites made with Sprite.Create are not scene objects, so Unity never
            // cleans them up on scene reload; destroy them manually to avoid leaking
            // memory across level restarts.
            foreach (Sprite sprite in createdSprites)
            {
                Destroy(sprite);
            }
        }

        private void PlayLossDelay()
        {
            SetPiecesInteractable(false);

            DOTween.Sequence()
                .AppendInterval(snapDuration + 0.5f)
                .AppendCallback(() => OnLevelLost?.Invoke())
                .SetLink(gameObject);
        }

        private void PlayWinCelebration()
        {
            SetPiecesInteractable(false);

            DOTween.Sequence()
                .AppendInterval(snapDuration)
                .Append(celebrationRoot.DOScale(1.03f, 0.25f).SetEase(Ease.OutQuad))
                .Append(celebrationRoot.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
                .AppendInterval(winCelebrationDuration)
                .AppendCallback(() => OnLevelWon?.Invoke())
                .SetLink(gameObject);
        }

        /// isGameOver only blocks drops; this also blocks picking pieces up,
        /// so nothing can be dragged during the end-of-level sequence.
        private void SetPiecesInteractable(bool interactable)
        {
            foreach (GridCoordinate coordinate in boardState.GetAllCoordinates())
            {
                PuzzlePiece piece = boardState.GetPieceAt(coordinate);
                if (piece != null)
                {
                    piece.GetComponent<CanvasGroup>().blocksRaycasts = interactable;
                }
            }
        }
    }
}