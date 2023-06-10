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

        private ChessEngine chessEngine;

        public MainWindow()
        {
            InitializeComponent();
            chessEngine = new(ChessField, 1, this);
            chessEngine.Setup();
        }

        
    }
}
