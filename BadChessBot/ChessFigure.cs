using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BadChessBot;

public abstract class ChessFigure 
{
    public Faction Faction { get; set; } = Faction.White;
    public Coordinate Position { get; internal set; }
    public abstract int FigureValue { get; }
    public abstract string FigureSpriteName { get; }
    public bool HasBeenMoved { get; set; } = false;
    public abstract bool IsAttacking(Coordinate target, ChessEngine engine);
    //public abstract Coordinate[] GetPossibleMoves(ChessEngine engine);
    public abstract bool CanMoveTo(Coordinate target, ChessEngine engine);

    private Image sprite;

    public Image Sprite => sprite;

    public ChessFigure(int x, int y, Faction faction, ChessEngine engine)
    {
        Position = new(x, y);
        Faction = faction;

        sprite = engine.CreateSpriteOnBoard(Position, FigureSpriteName);
    }
}