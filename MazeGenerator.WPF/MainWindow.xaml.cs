namespace MazeGenerator.WPF;

using MazeGenerator.Library;
using Microsoft.Win32;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MazeGenerator = MazeGenerator.Library.MazeGenerator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int _cellSize = 10;
    private int _solveStepDelay = 5;
    private MazeGenerator _mazeGenerator = new();
    private byte[,] _maze;
    private SKBitmap _bitmap;
    private SKPaint _wallPaint = new SKPaint() { Color = SKColors.Black };
    private float _scale = Resolution.GetScaleFactor();

    public MainWindow()
    {
        InitializeComponent();

        ColorPicker.Color = Colors.Blue;
    }

    private async void DrawPath((int, int) point, Color color)
    {
        int y = point.Item1;
        int x = point.Item2;

        using (var canvas = new SKCanvas(_bitmap))
        {
            var paint = new SKPaint { Color = ToSkiaColor(color), Style = SKPaintStyle.Fill };
            canvas.DrawRect(x * _cellSize * _scale, y * _cellSize * _scale, _cellSize * _scale, _cellSize * _scale, paint);
        }

        MazeView.InvalidateVisual();
        await Task.Delay(100);
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        if (_bitmap != null)
        {
            e.Surface.Canvas.DrawBitmap(_bitmap, 0, 0);
        }
    }

    private void DrawMaze(SKCanvas canvas, byte[,] maze)
    {
        for (int y = 0; y < maze.GetLength(0); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                if (maze[y, x] == 0)
                {
                    continue;
                }

                SKRect rect = new SKRect(x * _cellSize * _scale, y * _cellSize * _scale, (x + 1) * _cellSize * _scale, (y + 1) * _cellSize * _scale);

                canvas.DrawRect(rect, _wallPaint);
            }
        }
    }

    private SKColor ToSkiaColor(System.Windows.Media.Color color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if ((!int.TryParse(WidthInput.Text, out int width) && width < 5) || 
            (!int.TryParse(HeightInput.Text, out int height) && height < 5) || 
            (!int.TryParse(CellSizeInput.Text, out _cellSize) && (_cellSize > 100 || _cellSize < 1)))
        {
            MessageBox.Show("Please enter valid width, height and cell size values!");
            return;
        }

        try
        {
            checked
            {
                int size = width * height * _cellSize;
            }
        }
        catch (OverflowException)
        {
            MessageBox.Show(this, "The requested maze is too big to draw! Try using smaller cell size, or smaller dimensions!", "Your maze is way too big!");
            return;
        }

        StatusLabel.Content = "Generating Maze...";
        LoadingIcon.Visibility = Visibility.Visible;

        _maze = await _mazeGenerator.GenerateAsync(width, height);

        MazeView.Width = _maze.GetLength(1) * _cellSize;
        MazeView.Height = _maze.GetLength(0) * _cellSize;

        // Create a new bitmap for the updated maze dimensions
        _bitmap = new SKBitmap((int)(MazeView.Width * _scale), (int)(MazeView.Height * _scale));

        using var canvas = new SKCanvas(_bitmap);
        canvas.Clear(SKColors.White);
        await Task.Run(() => DrawMaze(canvas, _maze));

        MazeView.InvalidateVisual(); // Force a redraw of the maze

        StatusLabel.Content = "The Maze is Ready!";
        LoadingIcon.Visibility = Visibility.Collapsed;
    }

    private async void SolveMazeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_mazeGenerator == null)
        {
            MessageBox.Show("Please generate a maze first.");
            return;
        }

        if (!int.TryParse(DelayInput.Text, out _solveStepDelay))
        {
            _solveStepDelay = 0;
            DelayInput.Text = "0";
        }

        var timeStamp = System.Diagnostics.Stopwatch.StartNew();

        var height = _maze.GetLength(0);
        var width = _maze.GetLength(1);
        var startPoint = (1, 0);
        var endPoint = (height - 2, width - 1);

        List<(int, int)>? path = null;
        switch (PathFindModeComboBox.Text)
        {
            case "Shortest Path":
                StatusLabel.Content = "Finding shortest path...";
                path = await PathFinder.FindShortestPathAsync(_maze, startPoint, endPoint);
                break;
            case "First Path":
                StatusLabel.Content = "Finding first path...";
                path = await PathFinder.FindFirstPathAsync(_maze, startPoint, endPoint);
                break;
            case "Bidirectional":
                StatusLabel.Content = "Searching from both directions...";
                path = await PathFinder.FindBidirectionalPathAsync(_maze, startPoint, endPoint);
                break;
        }

        LoadingIcon.Visibility = Visibility.Visible;

        if (path is null)
        {
            MessageBox.Show("No path found.");
            return;
        }

        StatusLabel.Content = "Solving the Maze...";

        foreach (var point in path)
        {
            DrawPath(point, ColorPicker.Color);
            await Task.Delay(_solveStepDelay);
        }

        timeStamp.Stop();
        var endTime = timeStamp.Elapsed.TotalMilliseconds;

        StatusLabel.Content = $"Solution found in {(int)endTime}ms";
        LoadingIcon.Visibility = Visibility.Collapsed;
    }

    void SaveSKBitmapAsJpeg(SKBitmap bitmap, string filePath)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using Stream fileStream = File.OpenWrite(filePath);
        data.SaveTo(fileStream);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_bitmap is null)
        {
            StatusLabel.Content = "Generate a maze first!";
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "JPEG Files (*.jpg)|*.jpg|All Files (*.*)|*.*",
            DefaultExt = ".jpg",
            FileName = "maze"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            string filePath = saveFileDialog.FileName;
            SaveSKBitmapAsJpeg(_bitmap, filePath);
        }
    }
}