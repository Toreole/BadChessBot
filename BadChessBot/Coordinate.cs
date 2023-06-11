namespace BadChessBot;

public struct Coordinate
{
    public int x; 
    public int y;

    public Coordinate(int x, int y)
    {
        this.x = x; this.y = y;
    }

    public override readonly int GetHashCode()
    {
        return (x << 4) | y;
    }

    public readonly Coordinate Absolute()
    {
        return new(x<0?-x:x, y<0?-y:y);
    }

    public static Coordinate operator -(Coordinate a, Coordinate b) => new(a.x-b.x, a.y-b.y);
    public static Coordinate operator +(Coordinate a, Coordinate b) => new(a.x+b.x, a.y+b.y);

    public static bool operator ==(Coordinate a, Coordinate b) => a.x == b.x && a.y == b.y;

    public static bool operator !=(Coordinate a, Coordinate b) => a.x != b.x || a.y != b.y;

    public override readonly bool Equals(object? obj)
    {
        if (obj is Coordinate b)
            return this == b;
        return false;
    }
}
