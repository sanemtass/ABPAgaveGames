using System.Collections.Generic;

namespace AgavePuzzle.Gameplay
{
    public class ConnectionEvaluator
    {
        private readonly BoardState boardState;

        private readonly List<PuzzlePiece> groupBuffer = new List<PuzzlePiece>();
        private readonly HashSet<GridCoordinate> visitedBuffer = new HashSet<GridCoordinate>();
        private readonly Queue<GridCoordinate> queueBuffer = new Queue<GridCoordinate>();

        private static readonly GridCoordinate[] Directions =
        {
            new GridCoordinate(-1, 0), // up
            new GridCoordinate(0, 1),  // right
            new GridCoordinate(1, 0),  // down
            new GridCoordinate(0, -1), // left
        };

        public ConnectionEvaluator(BoardState boardState)
        {
            this.boardState = boardState;
        }

        public void RefreshAllConnections()
        {
            foreach (GridCoordinate coordinate in boardState.GetAllCoordinates())
            {
                PuzzlePiece piece = boardState.GetPieceAt(coordinate);
                if (piece == null)
                {
                    continue;
                }

                bool topConnected = IsConnected(piece, coordinate, Directions[0]);
                bool rightConnected = IsConnected(piece, coordinate, Directions[1]);
                bool bottomConnected = IsConnected(piece, coordinate, Directions[2]);
                bool leftConnected = IsConnected(piece, coordinate, Directions[3]);

                piece.SetBorderVisibility(
                    top: !topConnected,
                    right: !rightConnected,
                    bottom: !bottomConnected,
                    left: !leftConnected);
            }
        }

        public IReadOnlyList<PuzzlePiece> GetConnectedGroup(GridCoordinate startCoordinate)
        {
            groupBuffer.Clear();
            visitedBuffer.Clear();
            queueBuffer.Clear();

            queueBuffer.Enqueue(startCoordinate);
            visitedBuffer.Add(startCoordinate);

            while (queueBuffer.Count > 0)
            {
                GridCoordinate current = queueBuffer.Dequeue();
                PuzzlePiece currentPiece = boardState.GetPieceAt(current);

                if (currentPiece == null)
                {
                    continue;
                }

                groupBuffer.Add(currentPiece);

                foreach (GridCoordinate direction in Directions)
                {
                    if (!IsConnected(currentPiece, current, direction))
                    {
                        continue;
                    }

                    GridCoordinate neighborCoordinate = current + direction;

                    if (visitedBuffer.Contains(neighborCoordinate))
                    {
                        continue;
                    }

                    visitedBuffer.Add(neighborCoordinate);
                    queueBuffer.Enqueue(neighborCoordinate);
                }
            }

            return groupBuffer;
        }
        
        private bool IsConnected(PuzzlePiece piece, GridCoordinate coordinate, GridCoordinate direction)
        {
            GridCoordinate neighborCoordinate = coordinate + direction;

            PuzzlePiece neighborPiece = boardState.GetPieceAt(neighborCoordinate);
            if (neighborPiece == null)
            {
                return false;
            }

            GridCoordinate expectedNeighborCorrect = piece.CorrectCoordinate + direction;
            return expectedNeighborCorrect == neighborPiece.CorrectCoordinate;
        }
    }
}