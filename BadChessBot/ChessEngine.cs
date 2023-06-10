using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;

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

    public ChessEngine(Grid grid, int offset, MainWindow contextWindow)
    {
        this.chessGrid = grid;
        this.offset = offset;
        guiContext = contextWindow;
        contextWindow.BotRecommendationLabel.Content = "Nothing here quite yet";
    }

    public bool TryGetFigureAt(Coordinate coord, out ChessFigure? figure)
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
                    highlightedTiles.Add(tiles[x, y]);
                    tiles[x, y].Fill = Brushes.Orange;
                    selectedFigure = figure;
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
                rect.Fill = (Grid.GetColumn(rect) - offset) % 2 == (Grid.GetRow(rect) - offset) % 2
                    ? Brushes.ForestGreen : Brushes.Beige;
            }
            else
            {
                highlightedTiles.Add(rect);
                rect.Fill = Brushes.Orange;
            }

            
        }
    }

    private void ResetHighlights()
    {
        foreach (Rectangle highlighted in highlightedTiles)
        {
            highlighted.Fill = (Grid.GetColumn(highlighted) - offset) % 2 == (Grid.GetRow(highlighted) - offset) % 2
                ? Brushes.ForestGreen : Brushes.Beige;
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
    }

    internal bool FactionFigureAt(Coordinate coord, Faction faction)
    {
        if(TryGetFigureAt(coord, out ChessFigure? figure))
        {
            return figure!.Faction == faction;
        }
        return false;
    }
}
