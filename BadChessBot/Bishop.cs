namespace BadChessBot;

public class Bishop : ChessFigure
{
    public Bishop(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 3;

    public override string FigureSpriteName => Faction == Faction.White? "BishopSprite" : "BishopSprite2";

    public override bool IsAttacking(Coordinate target)
    {
        throw new System.NotImplementedException();
    }
}
