using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadChessBot;

internal class Rook : ChessFigure
{
    public Rook(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 4;

    public override string FigureSpriteName => Faction == Faction.White ? "RookSprite" : "RookSprite2";

    public override bool CanMoveTo(Coordinate target, ChessEngine engine)
    {
        return IsAttacking(target, engine);
    }

    public override bool IsAttacking(Coordinate target, ChessEngine engine)
    {
        if (target == Position) 
            return false;
        var off = target - Position;
        //not a straight line:
        if (off.x != 0 && off.y != 0) 
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
