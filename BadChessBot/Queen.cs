namespace BadChessBot;

public class Queen : ChessFigure
{
    public Queen(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 9;

    public override string FigureSpriteName => Faction == Faction.White ? "QueenSprite" : "QueenSprite2";

    public override bool IsAttacking(Coordinate target)
    {
        throw new System.NotImplementedException();
    }
}
