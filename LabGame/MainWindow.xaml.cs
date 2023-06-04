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
        }

        public class Cell
        {
            public int X { get; set; }
            public int Y { get; set; }
            public bool IsWall { get; set; }
            public bool IsStart { get; set; }
            public bool IsEnd { get; set; }
        }

        public class Maze
        {
            public Cell[,] Cells { get; set; }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            int width = int.Parse(widthTb.Text);
            int height = int.Parse(heightTb.Text);

            Maze maze;
            if (width <= 101 && height <= 55)
            {
                do
                {
                    maze = GenerateMaze(width, height);
                } while (HasLargeBlackBlock(maze));

                PlayGround gameWindow = new PlayGround();
                gameWindow.DisplayMaze(maze);
                gameWindow.Show();
            }
            else MessageBox.Show("Вы привысили максимальное значение сторон (H55 и W101)");
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
    }
}



//private void DivideMaze(Maze maze, int startX, int startY, int width, int height)
//{
//    if (width <= 2 || height <= 2)
//    {
//        return;  // The section is too small to divide further
//    }

//    // Determine where to draw the walls
//    int horizontalWallX = startX + width / 2;
//    int verticalWallY = startY + height / 2;

//    // Draw the walls
//    for (int x = startX; x < startX + width; x++)
//    {
//        maze.Cells[x, verticalWallY].IsWall = true;
//    }

//    for (int y = startY; y < startY + height; y++)
//    {
//        maze.Cells[horizontalWallX, y].IsWall = true;
//    }

//    // Randomly create holes in three of the walls
//    Random random = new Random();
//    for (int i = 0; i < 3; i++)
//    {
//        int wall = random.Next(4);
//        switch (wall)
//        {
//            case 0:  // Top wall
//                int holeX = random.Next(horizontalWallX, startX + width);
//                maze.Cells[holeX, verticalWallY].IsWall = false;
//                break;
//            case 1:  // Bottom wall
//                holeX = random.Next(startX, horizontalWallX);
//                maze.Cells[holeX, verticalWallY].IsWall = false;
//                break;
//            case 2:  // Left wall
//                int holeY = random.Next(verticalWallY, startY + height);
//                maze.Cells[horizontalWallX, holeY].IsWall = false;
//                break;
//            case 3:  // Right wall
//                holeY = random.Next(startY, verticalWallY);
//                maze.Cells[horizontalWallX, holeY].IsWall = false;
//                break;
//        }
//    }

//    // Recursively divide the four sub-sections of the maze
//    DivideMaze(maze, startX, startY, horizontalWallX - startX, verticalWallY - startY);  // Top left
//    DivideMaze(maze, horizontalWallX + 1, startY, startX + width - horizontalWallX - 1, verticalWallY - startY);  // Top right
//    DivideMaze(maze, startX, verticalWallY + 1, horizontalWallX - startX, startY + height - verticalWallY - 1);  // Bottom left
//    DivideMaze(maze, horizontalWallX + 1, verticalWallY + 1, startX + width - horizontalWallX - 1, startY + height - verticalWallY - 1);  // Bottom right
//}