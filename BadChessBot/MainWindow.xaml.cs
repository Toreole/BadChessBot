using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BadChessBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random random = new();

        public MainWindow()
        {
            InitializeComponent();
            SetupChessFieldTiles();
        }

        private void SetupChessFieldTiles()
        {
            for(int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var rect = new Rectangle
                    {
                        Fill = (i % 2) == (j % 2) ? Brushes.ForestGreen : Brushes.Beige,
                        Width = 50,
                        Height = 50
                    };
                    Grid.SetColumn(rect, i);
                    Grid.SetRow(rect, j);
                    ChessField.Children.Add(rect);
                    rect.MouseDown += ChessTileClicked;
                }
            }
        }

        private void ChessTileClicked(object sender, RoutedEventArgs e)
        {
            if(sender is Rectangle rec)
            {
                int x = Grid.GetColumn(rec);
                int y = Grid.GetRow(rec);
                var found = this.FindResource("PawnSprite");
                Image img = new() { Width = 50, Height = 50, Source = found as BitmapImage };
                Grid.SetColumn(img, x);
                Grid.SetRow(img, y);
                Panel.SetZIndex(img, 100);
                ChessField.Children.Add(img);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
