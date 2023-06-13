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
using System.Windows.Threading;

namespace LabGame
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += GameTimer_Tick;
        }

        public class Cell
        {
            public int X { get; set; }
            public int Y { get; set; }
            public bool IsWall { get; set; }
            public bool IsStart { get; set; }
            public bool IsEnd { get; set; }
            public bool IsPlayer { get; set; }
        }

        public class Maze
        {
            public Cell[,] Cells { get; set; }
            public Cell Player { get; set; }
        }
        private double cellSize = 14;
        private Maze maze;
        private bool isPlaying = false;
        private DispatcherTimer gameTimer;
        private DateTime startTime;


        private void GameTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - startTime;
            // Обновите текстовое поле с текущим временем игры, например:
            timerTextBlock.Text = elapsed.ToString(@"mm\:ss");
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            int width;
            int height;

            bool isWidthValid = int.TryParse(widthTb.Text, out width);
            bool isHeightValid = int.TryParse(heightTb.Text, out height);

            if (!isWidthValid || !isHeightValid)
            {
                MessageBox.Show("Введены некорректные значения. Пожалуйста, введите числовые значения для ширины и высоты.");
                return;
            }

            if (width % 2 == 0 || height % 2 == 0)
            {
                MessageBox.Show("Введены четные значения. Пожалуйста, введите нечетные значения для ширины и высоты.");
                return;
            }

            if (width > 101 || height > 55)
            {
                MessageBox.Show("Вы превысили максимальное значение сторон (высота 55 и длина 101).");
                return;
            }

            try
            {
                do
                {
                    this.maze = GenerateMaze(width, height);
                } while (HasLargeBlackBlock(this.maze));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при генерации лабиринта: {ex.Message}");
                return;
            }

            for (int x = 0; x < maze.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < maze.Cells.GetLength(1); y++)
                {
                    if (maze.Cells[x, y].IsStart)
                    {
                        maze.Player = maze.Cells[x, y];
                        maze.Player.IsPlayer = true;
                        break;
                    }
                }
            }

            StartMenuStackPanel.Visibility = Visibility.Collapsed;
            mazeCanvas.Visibility = Visibility.Visible;
            helpBar.Visibility = Visibility.Visible;
            option.Visibility = Visibility.Visible;
            if (mouseControl.IsChecked == true)
            {
                helpBar.Text = "Чтобы начать игру - переместите курсор в жёлтую клетку";
                DisplayMaze(this.maze);
            }
            else
            {
                isPlaying = true;
                helpBar.Text = "Время уже пошло, для перемещения используйте ←↑↓→";
                startTime = DateTime.Now;
                gameTimer.Start();
                DisplayMaze(this.maze);
            }
        }

        public void DisplayMaze(Maze maze)
        {
            this.maze = maze ?? throw new ArgumentNullException(nameof(maze));

            for (int x = 0; x < maze.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < maze.Cells.GetLength(1); y++)
                {
                    Cell cell = maze.Cells[x, y];

                    Rectangle rectangle = new Rectangle
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Fill = cell.IsStart ? Brushes.Yellow :
                               cell.IsEnd ? Brushes.Green :
                               cell.IsPlayer ? Brushes.Red :
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
            if (mouseControl.IsChecked != true)
            {
                return;
            }
            else
            {
                Point mousePosition = e.GetPosition(mazeCanvas);
                int x = (int)(mousePosition.X / cellSize);
                int y = (int)(mousePosition.Y / cellSize);

                if (x < 0 || y < 0 || x >= maze.Cells.GetLength(0) || y >= maze.Cells.GetLength(1))
                {
                    return;  // The mouse is outside the maze
                }

                Cell cell = maze.Cells[x, y];

                if (!isPlaying)
                {
                    // The game hasn't started yet. Check if the mouse is over the start cell
                    if (cell.IsStart)
                    {
                        isPlaying = true;  // Start the game
                    }
                    startTime = DateTime.Now;
                    gameTimer.Start();
                }
                else
                {
                    // The game has started. Check if the mouse is over a wall
                    if (cell.IsWall && !cell.IsStart && !cell.IsEnd)
                    {
                        gameTimer.Stop();
                        timerTextBlock.Text = "";
                        MessageBox.Show("Игра окончена! Вы врезались в стену!");

                        mazeCanvas.Children.Clear();
                        mazeCanvas.Visibility = Visibility.Collapsed;
                        option.Visibility = Visibility.Collapsed;
                        StartMenuStackPanel.Visibility = Visibility.Visible;
                        helpBar.Visibility = Visibility.Collapsed;
                        isPlaying = false;

                    }

                    // Check if the mouse is over the end cell
                    if (cell.IsEnd)
                    {
                        gameTimer.Stop();
                        TimeSpan finalTime = DateTime.Now - startTime;
                        timerTextBlock.Text = "";
                        MessageBox.Show($"Поздравляем! Вы прошли лабиринт за {finalTime.ToString(@"mm\:ss")}");

                        // Clear the canvas and return to the start menu
                        mazeCanvas.Children.Clear();
                        mazeCanvas.Visibility = Visibility.Collapsed;
                        option.Visibility = Visibility.Collapsed;
                        StartMenuStackPanel.Visibility = Visibility.Visible;
                        helpBar.Visibility = Visibility.Collapsed;
                        isPlaying = false;
                    }
                }
            }
        }

        private Maze GenerateMaze(int width, int height)
        {
            Maze maze = new Maze
            {
                Cells = new Cell[width, height]
            };

            // Initialize the maze cells as walls
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    maze.Cells[x, y] = new Cell { X = x, Y = y, IsWall = true };
                }
            }

            // Generate the maze using Prim's algorithm
            GenerateMazePrim(maze);

            // Find the first open cell for the start
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!maze.Cells[x, y].IsWall)
                    {
                        maze.Cells[x, y].IsStart = true;
                        goto EndStartLoop; // Break out of the nested loop
                    }
                }
            }
        EndStartLoop:

            // Find the last open cell for the end
            for (int x = width - 1; x >= 0; x--)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    if (!maze.Cells[x, y].IsWall)
                    {
                        maze.Cells[x, y].IsEnd = true;
                        return maze; // Return here since we've finished setting up the maze
                    }
                }
            }

            return maze; // This line should never be reached
        }

        private void GenerateMazePrim(Maze maze)
        {
            // Start from a random cell
            var random = new Random();
            int startX = random.Next(maze.Cells.GetLength(0));
            int startY = random.Next(maze.Cells.GetLength(1));
            maze.Cells[startX, startY].IsWall = false;

            // Create a list of walls
            var walls = new List<Cell>();
            AddWalls(maze, walls, startX, startY);

            // Process walls
            while (walls.Count > 0)
            {
                // Pick a random wall
                var wall = walls[random.Next(walls.Count)];
                walls.Remove(wall);

                // Check if it is a horizontal wall
                if (wall.X % 2 == 0 && wall.X > 0 && wall.X < maze.Cells.GetLength(0) - 1 && wall.IsWall)
                {
                    // Check the cells on the left and right side of the wall
                    var cell1 = maze.Cells[wall.X - 1, wall.Y];
                    var cell2 = maze.Cells[wall.X + 1, wall.Y];
                    if (cell1.IsWall && !cell2.IsWall)
                    {
                        // Make the wall a corridor and add the adjacent walls to the list
                        wall.IsWall = false;
                        cell1.IsWall = false;
                        AddWalls(maze, walls, cell1.X, cell1.Y);
                    }
                    else if (!cell1.IsWall && cell2.IsWall)
                    {
                        // Make the wall a corridor and add the adjacent walls to the list
                        wall.IsWall = false;
                        cell2.IsWall = false;
                        AddWalls(maze, walls, cell2.X, cell2.Y);
                    }
                }
                // Otherwise, it is a vertical wall
                else if (wall.Y % 2 == 0 && wall.Y > 0 && wall.Y < maze.Cells.GetLength(1) - 1 && wall.IsWall)
                {
                    // Check the cells above and below the wall
                    var cell1 = maze.Cells[wall.X, wall.Y - 1];
                    var cell2 = maze.Cells[wall.X, wall.Y + 1];
                    if (cell1.IsWall && !cell2.IsWall)
                    {
                        // Make the wall a corridor and add the adjacent walls to the list
                        wall.IsWall = false;
                        cell1.IsWall = false;
                        AddWalls(maze, walls, cell1.X, cell1.Y);
                    }
                    else if (!cell1.IsWall && cell2.IsWall)
                    {
                        // Make the wall a corridor and add the adjacent walls to the list
                        wall.IsWall = false;
                        cell2.IsWall = false;
                        AddWalls(maze, walls, cell2.X, cell2.Y);
                    }
                }
            }
        }

        private void AddWalls(Maze maze, List<Cell> walls, int x, int y)
        {
            // Add the horizontal walls
            if (x > 0 && maze.Cells[x - 1, y].IsWall) walls.Add(maze.Cells[x - 1, y]);
            if (x < maze.Cells.GetLength(0) - 1 && maze.Cells[x + 1, y].IsWall) walls.Add(maze.Cells[x + 1, y]);

            // Add the vertical walls
            if (y > 0 && maze.Cells[x, y - 1].IsWall) walls.Add(maze.Cells[x, y - 1]);
            if (y < maze.Cells.GetLength(1) - 1 && maze.Cells[x, y + 1].IsWall) walls.Add(maze.Cells[x, y + 1]);
        }

        private bool HasLargeBlackBlock(Maze maze)
        {
            for (int x = 0; x < maze.Cells.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < maze.Cells.GetLength(1) - 1; y++)
                {
                    if (maze.Cells[x, y].IsWall &&
                        maze.Cells[x + 1, y].IsWall &&
                        maze.Cells[x, y + 1].IsWall &&
                        maze.Cells[x + 1, y + 1].IsWall)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (keyControl.IsChecked != true)
            {
                return;
            }
            else
            {
                base.OnKeyDown(e);

                if (!isPlaying)
                {
                    return;
                }

                switch (e.Key)
                {
                    case Key.Up:
                        MovePlayer(0, -1);
                        break;

                    case Key.Down:
                        MovePlayer(0, 1);
                        break;

                    case Key.Left:
                        MovePlayer(-1, 0);
                        break;

                    case Key.Right:
                        MovePlayer(1, 0);
                        break;
                }
            }
        }

        private void MovePlayer(int dx, int dy)
        {
            int newX = maze.Player.X + dx;
            int newY = maze.Player.Y + dy;

            if (newX < 0 || newY < 0 || newX >= maze.Cells.GetLength(0) || newY >= maze.Cells.GetLength(1))
            {
                return; // Player tries to move out of the maze
            }

            if (maze.Cells[newX, newY].IsWall)
            {
                return; // Player tries to move into the wall
            }

            maze.Player.IsPlayer = false; // The old player's cell is no longer a player's cell
            maze.Player = maze.Cells[newX, newY]; // Move player
            maze.Player.IsPlayer = true;

            // Check if player has reached the end cell
            if (maze.Player.IsEnd)
            {
                gameTimer.Stop();
                TimeSpan finalTime = DateTime.Now - startTime;
                timerTextBlock.Text = "";
                MessageBox.Show($"Поздравляем! Вы прошли лабиринт за {finalTime.ToString(@"mm\:ss")}");

                mazeCanvas.Children.Clear();
                mazeCanvas.Visibility = Visibility.Collapsed;
                StartMenuStackPanel.Visibility = Visibility.Visible;
                helpBar.Visibility = Visibility.Collapsed;
                isPlaying = false;
            }

            DisplayMaze(maze); // Redraw the maze
        }


        private void option_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            timerTextBlock.Text = "";
            mazeCanvas.Children.Clear();
            mazeCanvas.Visibility = Visibility.Collapsed;
            StartMenuStackPanel.Visibility = Visibility.Visible;
            helpBar.Visibility = Visibility.Collapsed;
            isPlaying = false;
        }
    }
}