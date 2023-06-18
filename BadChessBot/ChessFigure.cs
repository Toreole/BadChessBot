using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BadChessBot;

public abstract class ChessFigure 
{
    public Faction Faction { get; set; } = Faction.White;

    private Coordinate currentPosition;
    private Coordinate previousPosition;

    public Coordinate Position
    {
        get => currentPosition; internal set
        {
            previousPosition = currentPosition;
            currentPosition = value;
        }
    }
    public Coordinate StartingPosition { get; }
    public abstract int FigureValue { get; }
    public abstract string FigureSpriteName { get; }
    public bool HasBeenMoved { get; set; } = false;
    public abstract bool IsAttacking(Coordinate target, ChessEngine engine);
    //public abstract Coordinate[] GetPossibleMoves(ChessEngine engine);
    public abstract bool CanMoveTo(Coordinate target, ChessEngine engine);

    public int GetDistanceValue(Coordinate target)
    {
        var offset = target - Position;
        var absOffset = offset.Absolute();
        int val = 0;
        if (this is Pawn)
        {
            val = absOffset.y + absOffset.x;
            //give value to promotions
            //if ((Faction == Faction.White && target.y == 7) || (Faction == Faction.Black && target.x == 0)) return 15;
        }
        if (this is Bishop) val = 1; //x/y doesnt matter its the same.
        if (this is Queen or Rook) val = 1;//absOffset.x == 0? absOffset.y : absOffset.y;
        if (this is Knight) val = 1;
        if (this is King) val = 0;
        if (target == StartingPosition) val--;
        if (target == previousPosition) val -= 2;

        return Math.Min(2,val);
    }

    private Image sprite;

    public Image Sprite => sprite;

    public ChessFigure(int x, int y, Faction faction, ChessEngine engine)
    {
        Position = new(x, y);
        StartingPosition = Position;
        Faction = faction;

        sprite = engine.CreateSpriteOnBoard(Position, FigureSpriteName);
    }

    protected ChessFigure()
    {
        sprite = new Image();
        StartingPosition = new(-1, -1);
    }
}