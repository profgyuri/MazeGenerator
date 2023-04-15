namespace MazeGenerator.Library;

using System;

public class MazeGenerator
{
    private int _width, _height;
    private int[,] _maze;
    private Random _random;

    public int Width => _width;
    public int Height => _height;
    public int[,] Maze => _maze;

    public MazeGenerator()
    {
        _maze = new int[_height, _width];
        _random = new Random();
    }

    public async Task<int[,]> GenerateAsync(int width, int height)
    {
        return await Task.Run(() => Generate(width, height));
    }

    private int[,] Generate(int width, int height)
    {
        _width = width % 2 == 0 ? width + 1 : width;
        _height = height % 2 == 0 ? height + 1 : height;

        _maze = new int[_height, _width];

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

        Stack<(int, int)> stack = new Stack<(int, int)>();

        int y = _random.Next(1, _height / 2) * 2 - 1;
        int x = _random.Next(1, _width / 2) * 2 - 1;
        _maze[y, x] = 0;
        stack.Push((y, x));

        int[] dy = { -1, 0, 1, 0 };
        int[] dx = { 0, 1, 0, -1 };

        while (stack.Count > 0)
        {
            (int cy, int cx) = stack.Peek();
            List<int> validDirections = new List<int>();

            for (int i = 0; i < 4; i++)
            {
                int ny = cy + dy[i] * 2;
                int nx = cx + dx[i] * 2;

                if (ny >= 0 && ny < _height && nx >= 0 && nx < _width && _maze[ny, nx] == 1)
                {
                    validDirections.Add(i);
                }
            }

            if (validDirections.Count > 0)
            {
                int dir = validDirections[_random.Next(validDirections.Count)];
                int ny = cy + dy[dir] * 2;
                int nx = cx + dx[dir] * 2;
                _maze[ny, nx] = 0;
                _maze[cy + dy[dir], cx + dx[dir]] = 0;
                stack.Push((ny, nx));
            }
            else
            {
                stack.Pop();
            }
        }

        _maze[1, 0] = 0;
        _maze[_height - 2, _width - 1] = 0;

        // Add extra connections to create multiple paths
        var maxConnections = (int)Math.Pow(Math.Log10((double)_width * _height), 2);
        int extraConnections = _random.Next(maxConnections / 2, maxConnections);

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

    private int CountWallIslandSize(int[,] maze, int y, int x, bool[,] visited, int threshold = -1)
    {
        visited[y, x] = true;

        int islandSize = 1;

        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int newY = y + dy[i];
            int newX = x + dx[i];

            if (newY >= 0 && newY < maze.GetLength(0) && newX >= 0 && newX < maze.GetLength(1) &&
                maze[newY, newX] == 1 && !visited[newY, newX])
            {
                islandSize += CountWallIslandSize(maze, newY, newX, visited, threshold);

                // Terminate early if the island size reaches the threshold
                if (threshold > 0 && islandSize >= threshold)
                {
                    return islandSize;
                }
            }
        }

        return islandSize;
    }

    private bool CanRemoveWall(int[,] maze, int y, int x)
    {
        // Remove the wall
        maze[y, x] = 0;

        bool[,] visited = new bool[maze.GetLength(0), maze.GetLength(1)];

        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int newY = y + dy[i];
            int newX = x + dx[i];

            if (maze[newY, newX] == 1 && !visited[newY, newX])
            {
                var minIslandSize = 15;
                int islandSize = CountWallIslandSize(maze, newY, newX, visited, minIslandSize);
                if (islandSize < minIslandSize)
                {
                    maze[y, x] = 1;
                    return false;
                }
            }
        }

        return true;
    }
}
