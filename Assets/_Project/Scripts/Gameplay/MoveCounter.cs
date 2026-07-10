using System;

namespace AgavePuzzle.Gameplay
{
    public class MoveCounter
    {
        public int MovesRemaining { get; private set; }

        public event Action<int> OnMovesChanged;

        public MoveCounter(int initialMoves)
        {
            MovesRemaining = initialMoves;
        }

        public void ConsumeMove()
        {
            if (MovesRemaining <= 0)
            {
                return;
            }

            MovesRemaining--;
            OnMovesChanged?.Invoke(MovesRemaining);
        }
    }
}