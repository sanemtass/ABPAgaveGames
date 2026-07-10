using System.Collections.Generic;
using UnityEngine;

namespace AgavePuzzle.Gameplay
{
    public class BoardState
    {
        private readonly PuzzlePiece[,] grid;

        public int Width { get; }
        public int Height { get; }

        public BoardState(int width, int height)
        {
            Width = width;
            Height = height;
            grid = new PuzzlePiece[height, width];
        }

        public bool IsWithinBounds(GridCoordinate coordinate)
        {
            return coordinate.Row >= 0 && coordinate.Row < Height &&
                   coordinate.Column >= 0 && coordinate.Column < Width;
        }

        public PuzzlePiece GetPieceAt(GridCoordinate coordinate)
        {
            if (!IsWithinBounds(coordinate))
            {
                return null;
            }

            return grid[coordinate.Row, coordinate.Column];
        }

        public void PlacePiece(PuzzlePiece piece, GridCoordinate coordinate)
        {
            if (!IsWithinBounds(coordinate))
            {
                Debug.LogError($"BoardState: {coordinate} is out of grid bounds, piece not placed.");
                return;
            }

            grid[coordinate.Row, coordinate.Column] = piece;
            piece.SetCurrentCoordinate(coordinate);
        }

        public void ClearAt(GridCoordinate coordinate)
        {
            if (!IsWithinBounds(coordinate))
            {
                return;
            }

            grid[coordinate.Row, coordinate.Column] = null;
        }

        public IEnumerable<GridCoordinate> GetAllCoordinates()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    yield return new GridCoordinate(row, column);
                }
            }
        }

        public bool AreAllPiecesCorrect()
        {
            foreach (GridCoordinate coordinate in GetAllCoordinates())
            {
                PuzzlePiece piece = GetPieceAt(coordinate);
                if (piece == null || !piece.IsInCorrectPosition)
                {
                    return false;
                }
            }

            return true;
        }
    }
}