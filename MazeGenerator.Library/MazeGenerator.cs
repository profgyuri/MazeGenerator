namespace MazeGenerator.Library;

using System;

public class MazeGenerator
{
    private int _width, _height;
    private byte[,] _maze;
    private Random _random;

    public int Width => _width;
    public int Height => _height;
    public byte[,] Maze => _maze;

    public MazeGenerator()
    {
        _maze = new byte[_height, _width];
        _random = new Random();
    }

    public async Task<byte[,]> GenerateAsync(int width, int height)
    {
        return await Task.Run(() => Generate(width, height));
    }

    private byte[,] Generate(int width, int height)
    {
        // Ensure the maze dimensions are odd to create a perfect maze
        _width = width % 2 == 0 ? width + 1 : width;
        _height = height % 2 == 0 ? height + 1 : height;

        _maze = new byte[_height, _width];

        // Initialize maze with walls
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                _maze[i, j] = 1;
            }
        }

        // Add walls on the sides
        for (int i = 0; i < _height; i++)
        {
            _maze[i, 0] = 1;
            _maze[i, _width - 1] = 1;
        }
        for (int j = 0; j < _width; j++)
        {
            _maze[0, j] = 1;
            _maze[_height - 1, j] = 1;
        }

        // Create a stack to store unvisited cells
        Stack<(int, int)> stack = new Stack<(int, int)>();

        // Choose a random starting point
        int y = _random.Next(1, _height / 2) * 2 - 1;
        int x = _random.Next(1, _width / 2) * 2 - 1;
        _maze[y, x] = 0; // Set the starting point as a path
        stack.Push((y, x));

        // Define possible movement directions
        int[] dy = { -1, 0, 1, 0 };
        int[] dx = { 0, 1, 0, -1 };

        // Depth-first search algorithm
        while (stack.Count > 0)
        {
            (int cy, int cx) = stack.Peek();
            List<int> validDirections = new List<int>();

            // Check for valid movement directions
            for (int i = 0; i < 4; i++)
            {
                int ny = cy + dy[i] * 2;
                int nx = cx + dx[i] * 2;

                if (ny >= 0 && ny < _height && nx >= 0 && nx < _width && _maze[ny, nx] == 1)
                {
                    validDirections.Add(i);
                }
            }

            // If there are valid directions, choose one at random and move
            if (validDirections.Count > 0)
            {
                int dir = validDirections[_random.Next(validDirections.Count)];
                int ny = cy + dy[dir] * 2;
                int nx = cx + dx[dir] * 2;
                _maze[ny, nx] = 0; // Set the new cell as a path
                _maze[cy + dy[dir], cx + dx[dir]] = 0; // Set the wall between the current cell and the new cell as a path
                stack.Push((ny, nx));
            }
            else // If there are no valid directions, backtrack
            {
                stack.Pop();
            }
        }

        // Create entrance and exit
        _maze[1, 0] = 0;
        _maze[_height - 2, _width - 1] = 0;

        // Add extra connections to create multiple paths
        var maxConnections = (int)Math.Pow(Math.Log10((double)_width * _height), 2);
        var extraConnections = _random.Next(maxConnections / 2, maxConnections);

        while (extraConnections > 0)
        {
            y = _random.Next(1, _height - 2);
            x = _random.Next(1, _width - 2);

            // Only break walls
            if (_maze[y, x] == 1)
            {
                // Check if there are two open cells horizontally or vertically
                if ((_maze[y - 1, x] == 0 && _maze[y + 1, x] == 0 && _maze[y, x - 1] != 0 && _maze[y, x + 1] != 0) ||
                    (_maze[y, x - 1] == 0 && _maze[y, x + 1] == 0 && _maze[y - 1, x] != 0 && _maze[y + 1, x] != 0))
                {
                    if (CanRemoveWall(_maze, y, x))
                    {
                        _maze[y, x] = 0;
                        extraConnections--;
                    }
                }
            }
        }

        return _maze;
    }

    private int CountWallIslandSize(byte[,] maze, int y, int x, bool[,] visited, int threshold = -1)
    {
        // Mark the current cell as visited
        visited[y, x] = true;

        // Initialize the island size counter
        int islandSize = 1;

        // Define possible movement directions
        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        // Traverse the neighboring cells
        for (int i = 0; i < 4; i++)
        {
            int newY = y + dy[i];
            int newX = x + dx[i];

            // Check if the neighboring cell is within the maze boundaries, has a wall, and hasn't been visited
            if (newY >= 0 && newY < maze.GetLength(0) && newX >= 0 && newX < maze.GetLength(1) &&
                maze[newY, newX] == 1 && !visited[newY, newX])
            {
                // Recursively count the size of the wall island
                islandSize += CountWallIslandSize(maze, newY, newX, visited, threshold);

                // Terminate early if the island size reaches the threshold
                if (threshold > 0 && islandSize >= threshold)
                {
                    return islandSize;
                }
            }
        }

        // Return the final island size
        return islandSize;
    }

    private bool CanRemoveWall(byte[,] maze, int y, int x)
    {
        // Remove the wall temporarily
        maze[y, x] = 0;

        // Create a visited matrix to track visited cells
        bool[,] visited = new bool[maze.GetLength(0), maze.GetLength(1)];

        // Define possible movement directions
        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        // Check the neighboring cells of the removed wall
        for (byte i = 0; i < 4; i++)
        {
            var newY = y + dy[i];
            var newX = x + dx[i];

            // Check if the neighboring cell has a wall and hasn't been visited
            if (maze[newY, newX] == 1 && !visited[newY, newX])
            {
                // Define the minimum island size threshold
                var minIslandSize = 15;

                // Check if removing the wall creates a wall island smaller than the threshold
                var islandSize = CountWallIslandSize(maze, newY, newX, visited, minIslandSize);
                if (islandSize < minIslandSize)
                {
                    // Put the wall back and return false (can't remove the wall)
                    maze[y, x] = 1;
                    return false;
                }
            }
        }

        // If no small wall islands are created, return true (can remove the wall)
        return true;
    }
}
