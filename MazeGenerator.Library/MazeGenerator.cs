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

        return _maze;
    }
}
