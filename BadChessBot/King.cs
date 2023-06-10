namespace BadChessBot;

public class King : ChessFigure
{
    public King(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 1000;

    public override string FigureSpriteName => Faction == Faction.White? "KingSprite" : "KingSprite2";

    public override bool IsAttacking(Coordinate target)
    {
        throw new System.NotImplementedException();
    }
}
