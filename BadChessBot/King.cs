namespace BadChessBot;

public class King : ChessFigure
{
    public King(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 1000;

    public override string FigureSpriteName => Faction == Faction.White? "KingSprite" : "KingSprite2";

    public override bool CanMoveTo(Coordinate target, ChessEngine engine)
    {
        return IsAttacking(target, engine) && engine.FactionFigureAt(target, Faction.OppositeFaction());
    }

    public override bool IsAttacking(Coordinate target, ChessEngine engine)
    {
        if (target == Position) return false;
        Coordinate off = (target - Position).Absolute();
        return off.x <= 1 && off.y <= 1;
    }
}
