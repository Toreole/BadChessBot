using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Documents;

namespace BadChessBot;

public class ChessEngine
{
    static readonly Coordinate[] tilesOnBoard;

    static ChessEngine()
    {
        tilesOnBoard = new Coordinate[64];
        for(int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                tilesOnBoard[x * 8 + y] = new(x, y);
            }
        }
    }

    readonly Grid chessGrid;
    readonly int offset = 0;
    readonly MainWindow guiContext;

    readonly Dictionary<Coordinate, ChessFigure> figuresOnTheBoard = new();
    readonly List<ChessFigure> takenFigures = new(32);
    readonly Rectangle[,] tiles = new Rectangle[8,8];

    readonly List<Rectangle> highlightedTiles = new List<Rectangle>(20);

    private ChessFigure? selectedFigure;

    //private Faction factionTurn = Faction.White;

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
                //figure!.Faction == factionTurn &&
                if ( (selectedFigure == null || figure!.Faction == selectedFigure.Faction))
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
        selectedFigure.HasBeenMoved = true;
        //deselect figure.
        selectedFigure = null;
        //set next players turn.
        //factionTurn = factionTurn.OppositeFaction();
        ResetHighlights();
        UpdateMoveRecommendation();
    }

    private void UpdateMoveRecommendation()
    {
        //guiContext.BotRecommendationLabel.Content = ...
        string recommendation = "";
        recommendation += "White: " + (GetMoveFor(Faction.White)?.ToString() ?? "none") + '\n';
        recommendation += "Black: " + (GetMoveFor(Faction.Black)?.ToString() ?? "none");
        guiContext.BotRecommendationLabel.Content = recommendation;
    }

    private Move? GetMoveFor(Faction faction)
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

    private Move? GetBestMoveForPieces(ChessFigure[] figures, King alliedKing)
    {
        Faction enemyFaction = alliedKing.Faction.OppositeFaction();
        King enemyKing = (King)figuresOnTheBoard.Values.First(x => x is King && x.Faction == enemyFaction);
        List<Move> allowedMoves = new();
        for (int i = 0; i < figures.Length && (i < 5 || allowedMoves.Count == 0); i++)
        {
            var figure = figures[i];
            foreach (var target in tilesOnBoard)
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
                        //add distance value to favor longer moves instead of always just moving pawns 1 piece.
                        move.ExpectedValue += figure.GetDistanceValue(target);

                        takenFigure = PreviewMove(move, out Coordinate originalPosition);
                        //add value to moves that do check (+1)
                        if(figures.Any(x => x.IsAttacking(enemyKing.Position, this)))
                        {
                            move.ExpectedValue += 1;
                            //add value to moves that do checkmate (+1000)
                            if(GetMoveFor(enemyFaction) == null)
                            {
                                move.ExpectedValue += 1000;
                            }
                        }
                        RevertMovePreview(move, originalPosition, takenFigure);
                    }
                }
            }
        }
        var selectedMove = allowedMoves.OrderByDescending(move => move.ExpectedValue).FirstOrDefault();
        return selectedMove;
    }

    private bool IsMoveLegal(Move move, King alliedKing)
    {
        //preview the move.
        ChessFigure? otherFigure = PreviewMove(move, out Coordinate orignalPosition);
        
        if (otherFigure != null && otherFigure.Faction == move.FigureToMove.Faction)
        {
            //cant take allied piece.
            RevertMovePreview(move, orignalPosition, otherFigure);
            return false;
        }
        //add value if the piece will be defended after moving:
        if (figuresOnTheBoard.Values.Any(f => f != move.FigureToMove && f.Faction == move.FigureToMove.Faction && f.IsAttacking(move.TargetPosition, this)))
        {
            move.ExpectedValue += 1;
        }
        //check check
        bool inCheck = figuresOnTheBoard.Values.Any(x => x.Faction != alliedKing.Faction && x.IsAttacking(alliedKing.Position, this));
        //revert the preview.
        RevertMovePreview(move, orignalPosition, otherFigure);
        return !inCheck;
    }

    private ChessFigure? PreviewMove(Move move, out Coordinate originalPosition)
    {
        ChessFigure? takenFigure = figuresOnTheBoard.ContainsKey(move.TargetPosition) ? figuresOnTheBoard[move.TargetPosition] : null;
        originalPosition = move.FigureToMove.Position;
        figuresOnTheBoard.Remove(originalPosition);
        move.FigureToMove.Position = move.TargetPosition;
        figuresOnTheBoard[move.TargetPosition] = move.FigureToMove;
        return takenFigure;
    }

    private void RevertMovePreview(Move move, Coordinate originalPosition, ChessFigure? takenFigure)
    {
        figuresOnTheBoard.Add(originalPosition, move.FigureToMove);
        move.FigureToMove.Position = originalPosition;
        if (takenFigure != null)
        {
            figuresOnTheBoard[move.TargetPosition] = takenFigure;
        }
        else
        {
            figuresOnTheBoard.Remove(move.TargetPosition);
        }
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
