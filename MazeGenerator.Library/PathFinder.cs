namespace MazeGenerator.Library;

public class PathFinder
{
    #region FindShortestPath
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
    #endregion

    #region FindFirstPath
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
    #endregion

    #region Bidirectional search
    public static async Task<List<(int, int)>?> FindBidirectionalPathAsync(int[,] maze, (int, int) startPoint, (int, int) endPoint)
    {
        var forwardTask = Task.Run(() => ExpandSearchAsync(maze, startPoint, endPoint));
        var backwardTask = Task.Run(() => ExpandSearchAsync(maze, endPoint, startPoint));

        var completedTask = await Task.WhenAny(forwardTask, backwardTask);

        var (meetingPoint, forwardPath, backwardPath) = completedTask.Result;

        if (meetingPoint == default) return null;

        backwardPath.Reverse();
        forwardPath.AddRange(backwardPath.Skip(1));
        return forwardPath;
    }

    private static async Task<((int, int) MeetingPoint, List<(int, int)> ForwardPath, List<(int, int)> BackwardPath)> ExpandSearchAsync(int[,] maze, (int, int) startPoint, (int, int) endPoint)
    {
        return await Task.Run(() =>
        {
            int[] dy = { -1, 0, 1, 0 };
            int[] dx = { 0, 1, 0, -1 };

            var height = maze.GetLength(0);
            var width = maze.GetLength(1);

            var visitedForward = new HashSet<(int, int)> { startPoint };
            var visitedBackward = new HashSet<(int, int)> { endPoint };

            var queueForward = new Queue<((int, int) Point, List<(int, int)> Path)>();
            var queueBackward = new Queue<((int, int) Point, List<(int, int)> Path)>();

            queueForward.Enqueue((startPoint, new List<(int, int)> { startPoint }));
            queueBackward.Enqueue((endPoint, new List<(int, int)> { endPoint }));

            List<(int, int)>? pathForward, pathBackward = null;

            while (queueForward.Count > 0 && queueBackward.Count > 0)
            {
                if (ExpandSearch(queueForward, visitedForward, visitedBackward, maze, dy, dx, out pathForward))
                {
                    pathBackward = queueBackward.First(entry => entry.Point == pathForward.Last()).Path;
                    return (pathForward.Last(), pathForward, pathBackward);
                }

                if (ExpandSearch(queueBackward, visitedBackward, visitedForward, maze, dy, dx, out pathBackward))
                {
                    pathForward = queueForward.First(entry => entry.Point == pathBackward.Last()).Path;
                    return (pathBackward.Last(), pathForward, pathBackward);
                }
            }

            return (default, null, null);
        });
    }

    private static bool ExpandSearch(Queue<((int, int) Point, List<(int, int)> Path)> queue, HashSet<(int, int)> visited, HashSet<(int, int)> otherVisited, int[,] maze, int[] dy, int[] dx, out List<(int, int)> path)
    {
        path = null;
        if (queue.Count == 0) return false;

        var current = queue.Dequeue();
        var point = current.Point;
        var currentPath = current.Path;

        for (int i = 0; i < 4; i++)
        {
            int newY = point.Item1 + dy[i];
            int newX = point.Item2 + dx[i];

            if (newY >= 0 && newY < maze.GetLength(0) && newX >= 0 && newX < maze.GetLength(1) && maze[newY, newX] == 0)
            {
                var nextPoint = (newY, newX);

                if (!visited.Contains(nextPoint))
                {
                    visited.Add(nextPoint);
                    var newPath = new List<(int, int)>(currentPath) { nextPoint };
                    queue.Enqueue((nextPoint, newPath));

                    if (otherVisited.Contains(nextPoint))
                    {
                        path = newPath;
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion
}