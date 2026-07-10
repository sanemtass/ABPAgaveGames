using System.Collections.Generic;

namespace AgavePuzzle.Gameplay
{
    public class GroupSwapService
    {
        private readonly BoardState boardState;

        private readonly HashSet<GridCoordinate> sourceCells = new HashSet<GridCoordinate>();
        private readonly List<GridCoordinate> groupTargets = new List<GridCoordinate>();
        private readonly List<PuzzlePiece> displacedPieces = new List<PuzzlePiece>();
        private readonly List<GridCoordinate> vacatedCells = new List<GridCoordinate>();
        private readonly List<GridCoordinate> displacedAssignments = new List<GridCoordinate>();

        public GroupSwapService(BoardState boardState)
        {
            this.boardState = boardState;
        }

        public bool TryExecuteGroupSwap(IReadOnlyList<PuzzlePiece> group, GridCoordinate delta)
        {
            sourceCells.Clear();
            groupTargets.Clear();
            displacedPieces.Clear();
            vacatedCells.Clear();
            displacedAssignments.Clear();

            // 1) Collect source cells, compute and validate target cells
            foreach (PuzzlePiece piece in group)
            {
                sourceCells.Add(piece.CurrentCoordinate);
            }

            foreach (PuzzlePiece piece in group)
            {
                GridCoordinate target = piece.CurrentCoordinate + delta;
                if (!boardState.IsWithinBounds(target))
                {
                    // Even one piece landing outside the grid cancels the swap (case rule)
                    return false;
                }

                groupTargets.Add(target);
            }

            // 2) Displaced pieces: occupants of target cells that don't belong to the group
            foreach (GridCoordinate target in groupTargets)
            {
                PuzzlePiece occupant = boardState.GetPieceAt(target);
                if (occupant != null && !sourceCells.Contains(occupant.CurrentCoordinate))
                {
                    displacedPieces.Add(occupant);
                }
            }

            // 3) Vacated cells: cells the group leaves behind and doesn't refill
            foreach (GridCoordinate source in sourceCells)
            {
                if (!sourceCells.Contains(source - delta))
                {
                    vacatedCells.Add(source);
                }
            }

            // 4) Assign displaced pieces to vacated cells:
            //    try the "corresponding" cell first, fall back to remaining cells in order
            foreach (PuzzlePiece displaced in displacedPieces)
            {
                GridCoordinate preferred = displaced.CurrentCoordinate - delta;
                if (vacatedCells.Remove(preferred))
                {
                    displacedAssignments.Add(preferred);
                }
                else
                {
                    displacedAssignments.Add(vacatedCells[0]);
                    vacatedCells.RemoveAt(0);
                }
            }

            // 5) Apply: clear all source cells first, then place every piece
            foreach (GridCoordinate source in sourceCells)
            {
                boardState.ClearAt(source);
            }

            for (int i = 0; i < group.Count; i++)
            {
                boardState.PlacePiece(group[i], groupTargets[i]);
            }

            for (int i = 0; i < displacedPieces.Count; i++)
            {
                boardState.PlacePiece(displacedPieces[i], displacedAssignments[i]);
            }

            return true;
        }
    }
}