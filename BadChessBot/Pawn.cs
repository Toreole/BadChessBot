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

    public override bool IsAttacking(Coordinate target)
    {
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
