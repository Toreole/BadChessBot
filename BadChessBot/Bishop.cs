using System;

namespace BadChessBot;

public class Bishop : ChessFigure
{
    public Bishop(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 3;

    public override string FigureSpriteName => Faction == Faction.White? "BishopSprite" : "BishopSprite2";

    public override bool IsAttacking(Coordinate target, ChessEngine engine)
    {
        if (target == Position)
            return false;
        var off = target - Position;
        var absOff = off.Absolute();
        //not a diagonal
        if (absOff.x != absOff.y) 
            return false;
        Coordinate direction = new(Math.Sign(off.x), Math.Sign(off.y));
        for(Coordinate next = Position + direction; next != target; next += direction)
        {
            if (!engine.FieldIsEmpty(next))
                return false;
        }
        return true;
    }
}
