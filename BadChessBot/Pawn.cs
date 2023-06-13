using System;
using System.Collections.Generic;

namespace BadChessBot;

public class Pawn : ChessFigure
{
    public Pawn(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 1;

    public override string FigureSpriteName => Faction==Faction.White? "PawnSprite" : "PawnSprite2";

    public override bool CanMoveTo(Coordinate target, ChessEngine engine)
    {
        if (target == Position) return false;
        var offset = target - Position;
        if(Faction == Faction.White)
        {
            return (offset.x == 0 && (offset.y == 1 || offset.y == 2 && !HasBeenMoved) && engine.FieldIsEmpty(target)) 
                || (offset.Absolute().x == 1 && offset.y == 1 && engine.FactionFigureAt(target, Faction.OppositeFaction()));
        }
        else
        {
            return (offset.x == 0 && (offset.y == -1 || offset.y == -2 && !HasBeenMoved) && engine.FieldIsEmpty(target))
                || (offset.Absolute().x == 1 && offset.y == -1 && engine.FactionFigureAt(target, Faction.OppositeFaction()));
        }
    }

    public override bool IsAttacking(Coordinate target, ChessEngine engine)
    {
        if (target == Position) return false;
        var offset = target - Position;
        if(Faction==Faction.White) //+y +/-x
        {
            return offset.y == 1 && offset.Absolute().x == 1;
        }
        else
        {

            return offset.y == -1 && offset.Absolute().x == 1;
        }
    }
}
