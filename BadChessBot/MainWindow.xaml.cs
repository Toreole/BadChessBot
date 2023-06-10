using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
                rec.Fill = new SolidColorBrush(new Color
                {
                    R = (byte)random.Next(255),
                    G = (byte)random.Next(255),
                    B = (byte)random.Next(255),
                    A = 255
                });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
