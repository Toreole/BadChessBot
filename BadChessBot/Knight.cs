﻿namespace BadChessBot;

public class Knight : ChessFigure
{
    public Knight(int x, int y, Faction faction, ChessEngine engine) : base(x, y, faction, engine)
    {
    }

    public override int FigureValue => 3;

    public override string FigureSpriteName => Faction == Faction.White? "KnightSprite" : "KnightSprite2";

    public override bool IsAttacking(Coordinate target)
    {
        throw new System.NotImplementedException();
    }
}
