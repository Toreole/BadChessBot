namespace BadChessBot;

public class Knight : ChessFigure
{
    public Knight(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 3;

    public override string FigureSpriteName => Faction == Faction.White? "KnightSprite" : "KnightSprite2";

    public override bool CanMoveTo(Coordinate target, ChessEngine engine)
    {
        return IsAttacking(target, engine) && engine.FactionFigureAt(target, Faction.OppositeFaction());
    }

    public override bool IsAttacking(Coordinate target, ChessEngine engine)
    {
        var absOff = (target - Position).Absolute();
        return (absOff.x == 2 && absOff.y == 1) || (absOff.x == 1 && absOff.y == 2);
    }
}
