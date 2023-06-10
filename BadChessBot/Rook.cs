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

    public override bool IsAttacking(Coordinate target)
    {
        throw new NotImplementedException();
    }
}
