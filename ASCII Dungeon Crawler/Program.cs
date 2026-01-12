using System;
using System.Collections.ObjectModel;
using System.IO.Pipelines;

class Program
{
    static readonly int width = 80;
    static readonly int height = 25;
    static readonly char[,] _map = new char[width, height];

    static readonly char playerSymbol = '@';
    static readonly char wallSymbol = '#';
    static readonly char floorSymbol = '.';

    static readonly int _playerXstart = 1, _playerYstart = 1;
    static int _playerX = _playerXstart, _playerY = _playerYstart;
    static readonly int startLives = 5;
    static int lives = startLives;

    static (int,int) _exitPoint = (width - 2, height - 2);
    static bool reachedExit = false;

    Random rand = new Random();

    static void Main(string[] args)
    {
        Game();
    }

    static void Game()
    {
        Console.CursorVisible = false;
        while (true) 
        {
            InitializeMap();
            reachedExit = false;
            while (!reachedExit)
            {
                DrawMap();
                HandleInput();
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
                    int chance = new Random().Next(0, 10);
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
        Console.SetCursorPosition(width + 2, 13);
        Console.WriteLine($"Lives: {lives}/{startLives}");
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
        Console.SetCursorPosition(width + 2, 1);
        Console.Write("Use W A S D to move the @ symbol to the exit.");

        Console.SetCursorPosition(width + 2, 3);
        Console.Write("Walls are represented by # symbols.");

        Console.SetCursorPosition(width + 2, 5);
        Console.Write("Floor is represented by . symbols.");

        Console.SetCursorPosition(_exitPoint.Item1 + 3, _exitPoint.Item2);
        Console.Write("< this is the exit");

    }

    static void DrawDynamicText()
    {
        Console.SetCursorPosition(width + 2, 13);
        Console.Write("                                                            ");
        Console.Write("Leben: ");
        for (int i = 0; i < lives; i++)
        {
            Console.Write("<3 ");
        }
    }
}