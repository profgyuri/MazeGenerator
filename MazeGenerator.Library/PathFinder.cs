namespace MazeGenerator.Library;

public class PathFinder
{
    public static async Task<List<(int, int)>?> FindShortestPathAsync(int[,] maze, (int, int) startPoint, (int, int) endPoint)
    {
        return await Task.Run(() => FindShortestPath(maze, startPoint, endPoint));
    }

    private static List<(int, int)> FindShortestPath(int[,] maze, (int, int) startPoint, (int, int) endPoint)
    {
        int[] dy = { -1, 0, 1, 0 };
        int[] dx = { 0, 1, 0, -1 };

        var height = maze.GetLength(0);
        var width = maze.GetLength(1);
        var visited = new bool[height, width];
        var queue = new Queue<((int, int) Point, List<(int, int)> Path)>();
        queue.Enqueue((startPoint, new List<(int, int)> { startPoint }));

        visited[startPoint.Item1, startPoint.Item2] = true;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var point = current.Point;
            var path = current.Path;

            if (point == endPoint)
            {
                return path;
            }

            for (int i = 0; i < 4; i++)
            {
                int newY = point.Item1 + dy[i];
                int newX = point.Item2 + dx[i];

                if (newY >= 0 && newY < height && newX >= 0 && newX < width &&
                    !visited[newY, newX] && maze[newY, newX] == 0)
                {
                    visited[newY, newX] = true;
                    var newPath = new List<(int, int)>(path) { (newY, newX) };
                    queue.Enqueue(((newY, newX), newPath));
                }
            }
        }

        return null;
    }

    public static async Task<List<(int, int)>?> FindFirstPathAsync(int[,] maze, (int, int) startPoint, (int, int) endPoint)
    {
        return await Task.Run(() => FindFirstPath(maze, startPoint, endPoint));
    }

    private static List<(int, int)> FindFirstPath(int[,] maze, (int, int) startPoint, (int, int) endPoint)
    {
        int[] dy = { -1, 0, 1, 0 };
        int[] dx = { 0, 1, 0, -1 };

        var height = maze.GetLength(0);
        var width = maze.GetLength(1);
        var visited = new bool[height, width];
        var stack = new Stack<((int, int) Point, List<(int, int)> Path)>();

        stack.Push((startPoint, new List<(int, int)> { startPoint }));
        visited[startPoint.Item1, startPoint.Item2] = true;

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            var point = current.Point;
            var path = current.Path;

            if (point == endPoint)
            {
                return path;
            }

            for (int i = 0; i < 4; i++)
            {
                int newY = point.Item1 + dy[i];
                int newX = point.Item2 + dx[i];

                if (newY >= 0 && newY < height && newX >= 0 && newX < width &&
                    !visited[newY, newX] && maze[newY, newX] == 0)
                {
                    visited[newY, newX] = true;
                    var newPath = new List<(int, int)>(path) { (newY, newX) };
                    stack.Push(((newY, newX), newPath));
                }
            }
        }

        return null;
    }
}