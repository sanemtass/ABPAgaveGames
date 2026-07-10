using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using AgavePuzzle.Data;

namespace AgavePuzzle.Gameplay
{
    public class PuzzleBoard : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] private LevelSequence levelSequence;

        [Header("Board References")]
        [SerializeField] private RectTransform boardContainer;
        [SerializeField] private PuzzlePiece piecePrefab;
        [SerializeField] private Image backgroundImage;

        [Header("Outer Frame")]
        [SerializeField] private Image frameTop;
        [SerializeField] private Image frameBottom;
        [SerializeField] private Image frameLeft;
        [SerializeField] private Image frameRight;

        [Header("Animation")]
        [SerializeField] private float snapDuration = 0.2f;

        private const float BoardAspectRatio = 9f / 16f;

        private LevelData levelData;
        private int gridWidth;
        private int gridHeight;
        private BoardState boardState;
        private ConnectionEvaluator connectionEvaluator;
        private GroupSwapService groupSwapService;
        private MoveCounter moveCounter;
        private bool isGameOver;

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

            ApplyFrameColor();
            BringFrameToFront();

            connectionEvaluator.RefreshAllConnections();
            OnBoardInitialized?.Invoke();
        }

        private void ApplyFrameColor()
        {
            Color frameColor = levelData.FrameColor;
            frameTop.color = frameColor;
            frameBottom.color = frameColor;
            frameLeft.color = frameColor;
            frameRight.color = frameColor;
        }

        private void BringFrameToFront()
        {
            // Frame bars exist in the scene before any piece is instantiated,
            // so without this they would render behind the pieces.
            frameTop.transform.SetAsLastSibling();
            frameBottom.transform.SetAsLastSibling();
            frameLeft.transform.SetAsLastSibling();
            frameRight.transform.SetAsLastSibling();
        }

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

            for (int i = coordinates.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (coordinates[i], coordinates[randomIndex]) = (coordinates[randomIndex], coordinates[i]);
            }

            return coordinates;
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
                OnLevelWon?.Invoke();
            }
            else if (moveCounter.MovesRemaining <= 0)
            {
                isGameOver = true;
                OnLevelLost?.Invoke();
            }
        }
    }
}