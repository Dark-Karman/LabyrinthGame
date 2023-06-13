using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LabGame
{
    /// <summary>
    /// Логика взаимодействия для PlayGround.xaml
    /// </summary>
    public partial class PlayGround : Window
    {
        private MainWindow.Maze maze;
        private const int cellSize = 15;
        private bool isPlaying = false;

        public PlayGround()
        {
            InitializeComponent();
        }

        public void DisplayMaze(MainWindow.Maze maze)
        {
            this.maze = maze ?? throw new ArgumentNullException(nameof(maze));

            for (int x = 0; x < maze.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < maze.Cells.GetLength(1); y++)
                {
                    MainWindow.Cell cell = maze.Cells[x, y];

                    Rectangle rectangle = new Rectangle
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Fill = cell.IsStart ? Brushes.Yellow :
                               cell.IsEnd ? Brushes.Green :
                               cell.IsWall ? Brushes.Black : Brushes.White
                    };

                    Canvas.SetLeft(rectangle, x * cellSize);
                    Canvas.SetTop(rectangle, y * cellSize);

                    mazeCanvas.Children.Add(rectangle);
                }
            }
        }


       

        private void mazeCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(mazeCanvas);
            int x = (int)(mousePosition.X / cellSize);
            int y = (int)(mousePosition.Y / cellSize);

            if (x < 0 || y < 0 || x >= maze.Cells.GetLength(0) || y >= maze.Cells.GetLength(1))
            {
                return;  // The mouse is outside the maze
            }

            MainWindow.Cell cell = maze.Cells[x, y];

            if (!isPlaying)
            {
                // The game hasn't started yet. Check if the mouse is over the start cell
                if (cell.IsStart)
                {
                    isPlaying = true;  // Start the game
                }
            }
            else
            {
                // The game has started. Check if the mouse is over a wall
                if (cell.IsWall && !cell.IsStart && !cell.IsEnd)
                {
                    MessageBox.Show("Game over! You hit a wall!");
                    this.Close();
                }

                // Check if the mouse is over the end cell
                if (cell.IsEnd)
                {
                    MessageBox.Show("Congratulations! You reached the end!");
                    this.Close();
                }
            }
        }
    }
}
