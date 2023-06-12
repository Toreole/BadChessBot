using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace BadChessBot;

public class ChessEngine
{
    readonly Grid chessGrid;
    readonly int offset = 0;
    readonly MainWindow guiContext;

    readonly Dictionary<Coordinate, ChessFigure> figuresOnTheBoard = new();
    readonly List<ChessFigure> takenFigures = new(32);
    readonly Rectangle[,] tiles = new Rectangle[8,8];

    readonly List<Rectangle> highlightedTiles = new List<Rectangle>(20);

    private ChessFigure? selectedFigure;

    private Faction factionTurn = Faction.White;

    public Grid ChessGrid => chessGrid;
    public MainWindow GUIContext => guiContext;

    Random random = new();

    public ChessEngine(Grid grid, int offset, MainWindow contextWindow)
    {
        this.chessGrid = grid;
        this.offset = offset;
        guiContext = contextWindow;
        contextWindow.BotRecommendationLabel.Content = "Nothing here quite yet";
    }

    public bool TryGetFigureAt(Coordinate coord, [MaybeNullWhen(false)] out ChessFigure? figure)
    {
        return figuresOnTheBoard.TryGetValue(coord, out figure);
    }

    public bool FieldIsEmpty(Coordinate coord)
    {
        return !figuresOnTheBoard.ContainsKey(coord);
    }

    public void Setup()
    {
        SetupChessFieldTiles();
        PlacePiecesInDefaultPositions();
        guiContext.RetryButton.Click += (_, _) => UpdateMoveRecommendation();
    }

    private void PlacePiecesInDefaultPositions()
    {
        //white pieces
        figuresOnTheBoard.Add(new(0, 0), new Rook(0, 0, Faction.White, this));
        figuresOnTheBoard.Add(new(1, 0), new Knight(1, 0, Faction.White, this));
        figuresOnTheBoard.Add(new(2, 0), new Bishop(2, 0, Faction.White, this));
        figuresOnTheBoard.Add(new(3, 0), new Queen(3, 0, Faction.White, this));
        figuresOnTheBoard.Add(new(4, 0), new King(4, 0, Faction.White, this));
        figuresOnTheBoard.Add(new(5, 0), new Bishop(5, 0, Faction.White, this));
        figuresOnTheBoard.Add(new(6, 0), new Knight(6, 0, Faction.White, this));
        figuresOnTheBoard.Add(new(7, 0), new Rook(7, 0, Faction.White, this));
        for (int x = 0; x < 8; x++)
            figuresOnTheBoard.Add(new(x, 1), new Pawn(x, 1, Faction.White, this));

        //black pieces
        figuresOnTheBoard.Add(new(0, 7), new Rook(0, 7, Faction.Black, this));
        figuresOnTheBoard.Add(new(1, 7), new Knight(1, 7, Faction.Black, this));
        figuresOnTheBoard.Add(new(2, 7), new Bishop(2, 7, Faction.Black, this));
        figuresOnTheBoard.Add(new(3, 7), new Queen(3, 7, Faction.Black, this));
        figuresOnTheBoard.Add(new(4, 7), new King(4, 7, Faction.Black, this));
        figuresOnTheBoard.Add(new(5, 7), new Bishop(5, 7, Faction.Black, this));
        figuresOnTheBoard.Add(new(6, 7), new Knight(6, 7, Faction.Black, this));
        figuresOnTheBoard.Add(new(7, 7), new Rook(7, 7, Faction.Black, this));
        for (int x = 0; x < 8; x++)
            figuresOnTheBoard.Add(new(x, 6), new Pawn(x, 6, Faction.Black, this));
    }

    internal Image CreateSpriteOnBoard(Coordinate coord, string spriteResourceKey)
    {
        var found = guiContext.FindResource(spriteResourceKey);
        Image img = new() { Width = 50, Height = 50, Source = found as BitmapImage };
        Grid.SetColumn(img, coord.x+offset);
        Grid.SetRow(img, coord.y+offset);
        Panel.SetZIndex(img, 100);
        chessGrid.Children.Add(img);
        img.MouseDown += ChessTileClicked;
        return img;
    }

    private void SetupChessFieldTiles()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var rect = new Rectangle
                {
                    Fill = (i % 2) == (j % 2) ? Brushes.ForestGreen : Brushes.Beige,
                    Width = 50,
                    Height = 50
                };
                Grid.SetColumn(rect, i + offset);
                Grid.SetRow(rect, j + offset);
                chessGrid.Children.Add(rect);
                rect.MouseDown += ChessTileClicked;
                tiles[i, j] = rect;
            }
        }
    }

    private void ChessTileClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Image image)
        {
            int x = Grid.GetColumn(image) - offset;
            int y = Grid.GetRow(image) - offset;
            Coordinate coord = new(x, y);
            if(TryGetFigureAt(coord, out var figure))
            {
                if(figure!.Faction == factionTurn && (selectedFigure == null || figure!.Faction == selectedFigure.Faction))
                {
                    ResetHighlights();
                    HighlightTile(tiles[x, y], Brushes.Yellow);
                    selectedFigure = figure;
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (selectedFigure.IsAttacking(new(i, j), this))
                                HighlightTile(tiles[i, j]);
                        }
                    }
                    foreach(var defender in figuresOnTheBoard.Values.Where(x => x.Faction == selectedFigure.Faction && x.IsAttacking(selectedFigure.Position, this)))
                    {
                        var defPos = defender.Position;
                        HighlightTile(tiles[defPos.x, defPos.y], Brushes.CadetBlue);
                    }
                }
                else if (selectedFigure != null && figure!.Faction != selectedFigure.Faction)
                {
                    ForceMove(coord);
                }
            }
        }
        else if(sender is Rectangle rect)
        {
            //move the selected figure.
            Coordinate targetCoord = new(Grid.GetColumn(rect) - offset, Grid.GetRow(rect) - offset);
            if(selectedFigure != null)
            {
                ForceMove(targetCoord);
                return;
            }
            if (highlightedTiles.Contains(rect))
            {
                highlightedTiles.Remove(rect);
                ResetDefaultColorTile(rect);
            }
            else
            {
                HighlightTile(rect);
            }

            
        }
    }

    private void ResetDefaultColorTile(Rectangle tile)
    {
        tile.Fill = (Grid.GetColumn(tile) - offset) % 2 == (Grid.GetRow(tile) - offset) % 2
                    ? Brushes.ForestGreen : Brushes.Beige;
    }

    private void HighlightTile(Rectangle tile, Brush? highlightBrush = null)
    {
        highlightedTiles.Add(tile);
        tile.Fill = highlightBrush ?? Brushes.Orange;
    }

    private void ResetHighlights()
    {
        foreach (Rectangle highlighted in highlightedTiles)
        {
            ResetDefaultColorTile(highlighted);
        }
        highlightedTiles.Clear();
    }

    private void ForceMove(Coordinate targetCoord)
    {
        if (selectedFigure == null) return;
        if (TryGetFigureAt(targetCoord, out var figure))
        {
            takenFigures.Add(figure!);
            figuresOnTheBoard.Remove(targetCoord);
            var takenSprite = figure!.Sprite;
            Grid.SetColumn(takenSprite, 0);
            Grid.SetRow(takenSprite, 0);
            chessGrid.Children.Remove(takenSprite);
            guiContext.TakenPiecesStackPanel.Children.Add(takenSprite);
        }
        var oldCoord = selectedFigure.Position;
        figuresOnTheBoard.Remove(oldCoord);
        figuresOnTheBoard.Add(targetCoord, selectedFigure);
        selectedFigure.Position = targetCoord;
        var sprite = selectedFigure.Sprite;
        Grid.SetColumn(sprite, targetCoord.x + offset);
        Grid.SetRow(sprite, targetCoord.y + offset);
        //deselect figure.
        selectedFigure = null;
        //set next players turn.
        factionTurn = factionTurn.OppositeFaction();
        ResetHighlights();
        UpdateMoveRecommendation();
    }

    private void UpdateMoveRecommendation()
    {
        //guiContext.BotRecommendationLabel.Content = ...
        string recommendation = "";
        recommendation += "White: " + GetMoveFor(Faction.White) + '\n';
        recommendation += "Black: " + GetMoveFor(Faction.Black);
        guiContext.BotRecommendationLabel.Content = recommendation;
    }

    private string GetMoveFor(Faction faction)
    {
        ChessFigure[] figuresInPlay = figuresOnTheBoard.Values.Where(x => x.Faction == faction).ToArray();
        King king = (King)figuresInPlay.Where(x => x is King).First();
        if(figuresInPlay.Length <= 3)
        {
            return GetBestMoveForPieces(figuresInPlay, king);
        }
        var chosenFigures = figuresInPlay.OrderBy(x => random.Next(100)).ToArray();
        return GetBestMoveForPieces(chosenFigures, king);
    }

    private string GetBestMoveForPieces(ChessFigure[] figures, King alliedKing)
    {
        List<Move> allowedMoves = new();
        for (int i = 0; i < figures.Length && i < 3; i++)
        {
            var figure = figures[i];
            foreach (var target in TileOnBoard())
            {
                if (figure.CanMoveTo(target, this))
                {
                    Move move = new(target, figure);
                    if (IsMoveLegal(move, alliedKing))
                    {
                        allowedMoves.Add(move);
                        //now calculate the expected value from this move.
                        //based on a few things:
                        //1. value of the piece being taken (if there is one)
                        //2. whether the target coord is attacked by the enemy AND defended by your own pieces.
                        if(TryGetFigureAt(target, out var takenFigure))
                        {
                            move.ExpectedValue += takenFigure!.FigureValue;
                        }
                        if(figuresOnTheBoard.Values.Any(f => f.Faction != figure.Faction && f.IsAttacking(target, this)))
                        {
                            move.ExpectedValue -= figure.FigureValue;
                        }
                        if(figuresOnTheBoard.Values.Any(f => f != figure && f.Faction == figure.Faction && f.IsAttacking(target, this)))
                        {
                            move.ExpectedValue += 1;
                        }
                    }
                }
            }
        }
        return allowedMoves.OrderByDescending(move => move.ExpectedValue).FirstOrDefault()?.ToString() ?? "no move";
    }

    private static IEnumerable<Coordinate> TileOnBoard()
    {
        for(int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                yield return new Coordinate(x, y);
    }

    private bool IsMoveLegal(Move move, King alliedKing)
    {
        //preview the move.
        figuresOnTheBoard.Remove(move.FigureToMove.Position);
        if (TryGetFigureAt(move.TargetPosition, out ChessFigure? otherFigure) && otherFigure!.Faction == move.FigureToMove.Faction)
        {
            //cant take allied piece.
            return false;
        }
        figuresOnTheBoard[move.TargetPosition] = move.FigureToMove;
        //check check
        bool inCheck = figuresOnTheBoard.Values.Any(x => x.Faction != alliedKing.Faction && x.IsAttacking(alliedKing.Position, this));
        //revert the preview.
        figuresOnTheBoard.Add(move.FigureToMove.Position, move.FigureToMove);
        if(otherFigure != null)
        {
            figuresOnTheBoard[move.TargetPosition] = otherFigure;
        }
        else
        {
            figuresOnTheBoard.Remove(move.TargetPosition);
        }
        return !inCheck;
    }

    public bool FactionAttacksTile(Faction faction, Coordinate tile)
    {
        foreach(var figure in figuresOnTheBoard.Values.Where(x => x.Faction == faction))
        {
            if (figure.IsAttacking(tile, this))
                return true;
        }
        return false;
    }

    internal bool FactionFigureAt(Coordinate coord, Faction faction)
    {
        if(TryGetFigureAt(coord, out ChessFigure? figure))
        {
            return figure!.Faction == faction;
        }
        return false;
    }

    private class Move
    {
        public Coordinate TargetPosition { get; }
        public ChessFigure FigureToMove { get; }
        public int ExpectedValue { get; set; }

        public Move(Coordinate targetPos, ChessFigure figure, int expectedValue = 0)
        {
            TargetPosition = targetPos;
            FigureToMove = figure;
            ExpectedValue = expectedValue;
        }

        /// <summary>
        /// returns the move as a readable string specifying the figure and target position
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{FigureToMove.GetType().Name} from {FigureToMove.Position} to {TargetPosition}";
        }
    }
}
