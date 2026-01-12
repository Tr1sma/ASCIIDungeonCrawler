using System;
using System.Collections.ObjectModel;
using System.IO.Pipelines;

class Program
{
    static readonly int width = 160;
    static readonly int height = 50;
    static readonly char[,] _map = new char[width, height];
    static readonly int wallDensity = 15; // Lower is denser

    static readonly char playerSymbol = '@';
    static readonly char wallSymbol = '#';
    static readonly char floorSymbol = '.';

    static readonly int _playerXstart = 1, _playerYstart = 1;
    static int _playerX = _playerXstart, _playerY = _playerYstart;
    static readonly int startLives = 5;
    static int lives = startLives;

    static (int,int) _exitPoint = (width - 2, height - 2);
    static bool reachedExit = false;

    static bool dynamicMode = false;
    static readonly int mapShuffleInterval = 25; // Value is getting Multiplied by frame time (16ms)
    static int cycleCount = 0;

    Random rand = new Random();

    static void Main(string[] args)
    {
        Game();
    }

    static void Game()
    {
        Console.CursorVisible = false;
        if(Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            Console.WindowHeight = height + 1;
            Console.WindowWidth = width + 1;

        }
        while (true) 
        {
            InitializeMap();
            reachedExit = false;
            while (!reachedExit)
            {
                if (dynamicMode)
                {
                    ShuffleMap();
                }
                DrawMap();
                
                if (Console.KeyAvailable) HandleInput();

                System.Threading.Thread.Sleep(16); // ~60 FPS
                cycleCount++;
                if(cycleCount > mapShuffleInterval)
                {
                    cycleCount = 0;
                }
            }
        }
    }

    static void ShuffleMap()
    {
        if (cycleCount != mapShuffleInterval) return;

        char[,] nextMap = new char[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    nextMap[x, y] = wallSymbol;
                else
                    nextMap[x, y] = floorSymbol;
            }
        }

        var walls = new System.Collections.Generic.List<(int x, int y)>();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (_map[x, y] == wallSymbol)
                {
                    walls.Add((x, y));
                }
            }
        }

        Random rng = new Random();
        int n = walls.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = walls[k];
            walls[k] = walls[n];
            walls[n] = value;
        }

        foreach (var (wx, wy) in walls)
        {
            var validMoves = new System.Collections.Generic.List<(int x, int y)>();
            
            (int dx, int dy)[] directions = { (0, -1), (0, 1), (-1, 0), (1, 0) };

            foreach (var dir in directions)
            {
                int nx = wx + dir.dx;
                int ny = wy + dir.dy;

                if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1)
                {
                    if (_map[nx, ny] == floorSymbol && 
                        nextMap[nx, ny] == floorSymbol && 
                        !(nx == _playerX && ny == _playerY))
                    {
                        validMoves.Add((nx, ny));
                    }
                }
            }

            if (validMoves.Count > 0)
            {
                var move = validMoves[rng.Next(validMoves.Count)];
                nextMap[move.x, move.y] = wallSymbol;
            }
            else
            {
                nextMap[wx, wy] = wallSymbol;
            }
        }

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                _map[x, y] = nextMap[x, y];
            }
        }
    }

    static void InitializeMap()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (i == 0 || j == 0 || i == width - 1 || j == height - 1)
                    _map[i, j] = wallSymbol;
                else
                {
                    int chance = new Random().Next(0, wallDensity);
                    if (chance == 0)
                        _map[i, j] = wallSymbol;
                    else
                        _map[i, j] = floorSymbol;
                }
            }
        }
        _map[_playerX, _playerY] = floorSymbol;
        _map[width - 1, height - 2] = floorSymbol; // Exit point
        DrawStaticText();
    }

    static void DrawMap()
    {
        Console.SetCursorPosition(0, 0);
        var buffer = new System.Text.StringBuilder();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == _playerX && y == _playerY)
                {
                    buffer.Append(playerSymbol);
                }
                else
                {
                    buffer.Append(_map[x, y]);
                }
            }
            buffer.AppendLine();
        }
        Console.Write(buffer.ToString());
        DrawDynamicText();
    }

    static void HandleInput()
    {
        var key = Console.ReadKey(true).Key;
        int dx = 0, dy = 0;

        switch (key)
        {
            case ConsoleKey.W: dy = -1; break;
            case ConsoleKey.S: dy = 1; break;
            case ConsoleKey.A: dx = -1; break;
            case ConsoleKey.D: dx = 1; break;
            case ConsoleKey.D1: dynamicMode = !dynamicMode; break;
        }

        try
        {
            if (_map[_playerX + dx, _playerY + dy] != '#')
            {
                _playerX += dx;
                _playerY += dy;
            }
            else
            {
                //Do notheing for now
                //lives--;
            }
        }
        catch (IndexOutOfRangeException)
        {
            Console.Clear();
            _playerX = _playerXstart;
            _playerY = _playerYstart;
            reachedExit = true;
        }
    }

    static void DrawStaticText()
    {
        try
        {
            Console.SetCursorPosition(width + 2, 1);
            Console.Write("Use W A S D to move the @ symbol to the exit.");

            Console.SetCursorPosition(width + 2, 3);
            Console.Write("Walls are represented by # symbols.");

            Console.SetCursorPosition(width + 2, 5);
            Console.Write("Floor is represented by . symbols.");

            Console.SetCursorPosition(width + 2, 7);
            Console.Write("Press [1] to toggle Dynamic mode");

            Console.SetCursorPosition(_exitPoint.Item1 + 3, _exitPoint.Item2);
            Console.Write("< this is the exit");
        }
        catch { }
    }

    static void DrawDynamicText()
    {
        try
        {
            Console.SetCursorPosition(width + 2, 13);
            Console.WriteLine($"Lives: {lives}/{startLives}");
        }
        catch { }
    }
}