using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

class Program
{
    static readonly int width = 80;
    static readonly int height = 25;
    static readonly char[,] _map = new char[width, height];
    static readonly int wallDensity = 5; // Lower is denser
    static readonly int lootDensity = 150; // Lower is denser

    static readonly char playerSymbol = '@';
    static readonly char wallSymbol = '#';
    static readonly char floorSymbol = '.';
    static readonly char lootBox = 'X';

    static readonly int _playerXstart = 1, _playerYstart = 1;
    static int _playerX = _playerXstart, _playerY = _playerYstart;
    static readonly int startLives = 5;
    private static readonly int lives = startLives;

    static (int width, int height) _exitPoint = (width - 2, height - 2);
    static bool reachedExit = false;

    static bool dynamicMode = false;
    static readonly int mapShuffleInterval = 25; // Value is getting Multiplied by frame time (16ms)
    static int cycleCount = 0;

    private static readonly string[] instructions =
    [
        "Use W A S D to move the @ symbol to the exit.",
        "Walls are represented by # symbols.",
        "Floor is represented by . symbols.",
        "Lootboxes are represented by X symbols.",
        "Press [1] to toggle Dynamic mode"
    ];

    static (int wall, int loot) WallLootCounter = (0, 0);

    static readonly Random rnd = new();

    static readonly NativeConsoleListener _inputListener = new();

    static void Main()
    {
        Game();
    }

    static void Game()
    {
        #if WINDOWS
            Console.BufferHeight = height + 5;
            Console.BufferWidth = width + instructions.OrderByDescending(x => x.Length).First().Length + 3;
        #endif

        Console.CursorVisible = false;
        while (true)
        {
            InitializeMap();
            reachedExit = false;
            while (!reachedExit)
            {
                if (dynamicMode) ShuffleMap();

                DrawMap();
                _inputListener.ProcessInput(
                    onKeyDown: (key) => HandleInput(key),
                    onResize: () => DrawStaticText()
                );

                System.Threading.Thread.Sleep(16);
                cycleCount++;
                if (cycleCount > mapShuffleInterval) cycleCount = 0;
            }
        }
    }



    static void HandleInput(ConsoleKey key)
    {
        int dx, dy;

        if (key == ConsoleKey.D1) dynamicMode = !dynamicMode;

        (dx, dy) = key switch
        {
            ConsoleKey.W => (0, -1),
            ConsoleKey.S => (0, 1),
            ConsoleKey.A => (-1, 0),
            ConsoleKey.D => (1, 0),
            _ => (0, 0)
        };

        MovePlayer(dx, dy);
    }

    static void MovePlayer(int dx, int dy)
    {
        if (dx == 0 && dy == 0) return;
        try
        {
            if (_map[_playerX + dx, _playerY + dy] != wallSymbol && _map[_playerX + dx, _playerY + dy] != lootBox)
            {
                _playerX += dx;
                _playerY += dy;
            }
            if (_map[_playerX + dx, _playerY + dy] == lootBox)
            {
                _playerX += dx;
                _playerY += dy;
                _map[_playerX, _playerY] = floorSymbol;
                WallLootCounter.loot--;
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
                    _map[i, j] = (rnd.Next(0, wallDensity) == 0) ? wallSymbol : floorSymbol;

                    _map[i, j] = (rnd.Next(0, lootDensity) == 0) ? lootBox : _map[i, j];

                    if (_map[i, j] == wallSymbol)
                        WallLootCounter.wall++;
                    else if (_map[i, j] == lootBox)
                        WallLootCounter.loot++;
                }
            }
        }
        _map[_playerX, _playerY] = floorSymbol;
        _map[width - 1, height - 2] = floorSymbol;
        DrawStaticText();
    }

    static void ShuffleMap()
    {
        if (cycleCount != mapShuffleInterval) return;

        char[,] nextMap = new char[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nextMap[x, y] = (x == 0 || y == 0 || x == width - 1 || y == height - 1) ? wallSymbol : floorSymbol;
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

        Random rng = new();
        int n = walls.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (walls[n], walls[k]) = (walls[k], walls[n]);
        }

        foreach (var (wx, wy) in walls)
        {
            var validMoves = new System.Collections.Generic.List<(int x, int y)>();

            (int dx, int dy)[] directions = [(0, -1), (0, 1), (-1, 0), (1, 0)];

            foreach (var (dx, dy) in directions)
            {
                int nx = wx + dx;
                int ny = wy + dy;

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
                var (x, y) = validMoves[rng.Next(validMoves.Count)];
                nextMap[x, y] = wallSymbol;
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

    static void DrawMap()
    {
        Console.SetCursorPosition(0, 0);
        var buffer = new System.Text.StringBuilder();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == _playerX && y == _playerY) buffer.Append(playerSymbol);
                else buffer.Append(_map[x, y]);
            }
            buffer.AppendLine();
        }
        Console.Write(buffer.ToString());
        DrawDynamicText();
    }

    static void DrawStaticText()
    {
        try
        {
            if (Console.WindowWidth < width + 2) return;
            for (int i = 0; i < instructions.Length; i++)
            {
                Console.SetCursorPosition(width + 2, 1 + i * 2);
                Console.Write(instructions[i]);
            }
            Console.SetCursorPosition(_exitPoint.width + 3, _exitPoint.height);
            Console.Write("< Exit");
        }
        catch { }
    }

    static void DrawDynamicText()
    {
        try
        {
            if (Console.WindowWidth < width + 2) return;
            Console.SetCursorPosition(width + 2, 13);
            Console.WriteLine($"Lives: {lives}/{startLives} ");

            Console.SetCursorPosition(width + 2, 15);
            Console.WriteLine($"Walls: {WallLootCounter.wall}  ");
            Console.SetCursorPosition(width + 2, 16);
            Console.WriteLine($"Lootboxes: {WallLootCounter.loot}  ");
        }
        catch { }
    }
}
