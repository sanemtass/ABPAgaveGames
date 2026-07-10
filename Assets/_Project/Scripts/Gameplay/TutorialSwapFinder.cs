using System.Collections.Generic;

namespace AgavePuzzle.Gameplay
{
    /// Finds a "connecting swap" on the current board: swapping two single
    /// pieces so that at least one new connection is created (case rule).
    public class TutorialSwapFinder
    {
        private readonly BoardState boardState;
        private readonly ConnectionEvaluator connectionEvaluator;

        private static readonly GridCoordinate[] Directions =
        {
            new GridCoordinate(-1, 0), // up
            new GridCoordinate(0, 1),  // right
            new GridCoordinate(1, 0),  // down
            new GridCoordinate(0, -1), // left
        };

        public TutorialSwapFinder(BoardState boardState, ConnectionEvaluator connectionEvaluator)
        {
            this.boardState = boardState;
            this.connectionEvaluator = connectionEvaluator;
        }

        public bool TryFindConnectingSwap(out GridCoordinate from, out GridCoordinate to)
        {
            foreach (GridCoordinate a in boardState.GetAllCoordinates())
            {
                // The dragged piece must be a single (group of 1) so the demo
                // matches what actually happens when the player drags it.
                if (connectionEvaluator.GetConnectedGroup(a).Count != 1)
                {
                    continue;
                }

                foreach (GridCoordinate b in boardState.GetAllCoordinates())
                {
                    if (a == b)
                    {
                        continue;
                    }
                    // Target must also be a single piece: pointing the player at a cell
                    // inside an already-connected group would teach them to break it.
                    if (connectionEvaluator.GetConnectedGroup(b).Count != 1)
                    {
                        continue;
                    }
                    if (SwapCreatesNewConnection(a, b))
                    {
                        from = a;
                        to = b;
                        return true;
                    }
                }
            }

            from = default;
            to = default;
            return false;
        }

        private bool SwapCreatesNewConnection(GridCoordinate a, GridCoordinate b)
        {
            // Only edges touching cell a or cell b can change; check those.
            return HasNewConnectionAround(a, a, b) || HasNewConnectionAround(b, a, b);
        }

        private bool HasNewConnectionAround(GridCoordinate cell, GridCoordinate a, GridCoordinate b)
        {
            foreach (GridCoordinate direction in Directions)
            {
                GridCoordinate neighbor = cell + direction;
                if (!boardState.IsWithinBounds(neighbor))
                {
                    continue;
                }

                bool connectedBefore = AreConnected(
                    boardState.GetPieceAt(cell), boardState.GetPieceAt(neighbor), direction);
                bool connectedAfter = AreConnected(
                    GetPieceAfterSwap(cell, a, b), GetPieceAfterSwap(neighbor, a, b), direction);

                if (connectedAfter && !connectedBefore)
                {
                    return true;
                }
            }

            return false;
        }

        /// Which piece would occupy the given cell after swapping the pieces at a and b.
        private PuzzlePiece GetPieceAfterSwap(GridCoordinate cell, GridCoordinate a, GridCoordinate b)
        {
            if (cell == a) return boardState.GetPieceAt(b);
            if (cell == b) return boardState.GetPieceAt(a);
            return boardState.GetPieceAt(cell);
        }

        private static bool AreConnected(PuzzlePiece piece, PuzzlePiece neighborPiece, GridCoordinate direction)
        {
            if (piece == null || neighborPiece == null)
            {
                return false;
            }

            return piece.CorrectCoordinate + direction == neighborPiece.CorrectCoordinate;
        }
    }
}