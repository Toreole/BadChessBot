﻿using System;

namespace BadChessBot;

public class Queen : ChessFigure
{
    public Queen(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 9;

    public override string FigureSpriteName => Faction == Faction.White ? "QueenSprite" : "QueenSprite2";

    public override bool CanMoveTo(Coordinate target, ChessEngine engine)
    {
        return IsAttacking(target, engine) && engine.FactionFigureAt(target, Faction.OppositeFaction());
    }

    public override bool IsAttacking(Coordinate target, ChessEngine engine)
    {
        if (target == Position)
            return false;
        var off = target - Position;
        var absOff = off.Absolute();
        //not a diagonal and also not a straight
        if ((absOff.x != absOff.y) && (absOff.x != 0 && absOff.y != 0))
            return false;
        Coordinate direction = new(Math.Sign(off.x), Math.Sign(off.y));
        for (Coordinate next = Position + direction; next != target; next += direction)
        {
            if (!engine.FieldIsEmpty(next))
                return false;
        }
        return true;
    }
}
