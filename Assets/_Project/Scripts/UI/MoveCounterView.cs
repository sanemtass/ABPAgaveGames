using TMPro;
using UnityEngine;
using AgavePuzzle.Gameplay;

namespace AgavePuzzle.UI
{
    public class MoveCounterView : MonoBehaviour
    {
        [SerializeField] private PuzzleBoard puzzleBoard;
        [SerializeField] private TextMeshProUGUI moveCountText;

        private void OnEnable()
        {
            puzzleBoard.OnBoardInitialized += HandleBoardInitialized;
        }

        private void OnDisable()
        {
            puzzleBoard.OnBoardInitialized -= HandleBoardInitialized;

            if (puzzleBoard.MoveCounter != null)
            {
                puzzleBoard.MoveCounter.OnMovesChanged -= UpdateMoveText;
            }
        }

        private void HandleBoardInitialized()
        {
            puzzleBoard.MoveCounter.OnMovesChanged += UpdateMoveText;
            UpdateMoveText(puzzleBoard.MoveCounter.MovesRemaining);
        }

        private void UpdateMoveText(int movesRemaining)
        {
            moveCountText.text = $"Moves: {movesRemaining}";
        }
    }
}