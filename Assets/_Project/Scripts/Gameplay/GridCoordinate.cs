using System;

namespace AgavePuzzle.Gameplay
{
    public struct GridCoordinate : IEquatable<GridCoordinate>
    {
        public int Row { get; }
        public int Column { get; }

        public GridCoordinate(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool Equals(GridCoordinate other) => Row == other.Row && Column == other.Column;

        public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b) =>
            new GridCoordinate(a.Row + b.Row, a.Column + b.Column);

        public static GridCoordinate operator -(GridCoordinate a, GridCoordinate b) =>
            new GridCoordinate(a.Row - b.Row, a.Column - b.Column);

        public override bool Equals(object obj) => obj is GridCoordinate other && Equals(other);
        public override int GetHashCode() => (Row, Column).GetHashCode();

        public static bool operator ==(GridCoordinate a, GridCoordinate b) => a.Equals(b);
        public static bool operator !=(GridCoordinate a, GridCoordinate b) => !a.Equals(b);

        public override string ToString() => $"({Row}, {Column})";
    }
}