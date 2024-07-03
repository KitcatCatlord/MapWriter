using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// NOTES:
//File location is C:\Users\kianc\source\repos\Map Editor V1.3\Map Editor V1.3\bin\Debug

//For sector change, do ChangeSecrtor(icnrement). The whole thing should be a 5x5 grid, so you start at top left, and going down adds 5 to your sector and going up subtracts 5. Set y to bottom or top when going up or down accordingly, and keep the x coordinate.
//Edge of map cursor position is 29
//Add wall movement prevention
//Make torches only show up when they are in line of sight (can use same code as lighting behind wall calculation when made)

class Program
{
    static int sectorSize = 30;
    static int[,] map = new int[sectorSize, sectorSize];
    static int cursorX = 15;// DEFAULT 0
    static int cursorY = 0; // DEFAULT 0
    static int currentSector = 7; // DEFAULT 0
    static bool inCave = true; // DEFAULT FALSE
    static bool isLightBehindWall = false;

    static int lightingLevel4Default = 237;
    static int lightingLevel3Default = 242;
    static int lightingLevel2Default = 248;
    static int lightingLevel1Default = 253;
    
    static int lightingLevel4 = 237;
    static int lightingLevel3 = 242;
    static int lightingLevel2 = 248;
    static int lightingLevel1 = 253;

    static int lightingTimer = 0;
    static bool renderingLighting = false;

    // COLOUR STUFF
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetConsoleMode(IntPtr handle, out int mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(int handle);
    // COLOUR STUFF
    static void Main(string[] args)
    {
        Console.WriteLine("Please maximise your window, then press ENTER.");
        Console.ReadLine();
        Console.Clear();
        Console.CursorVisible = false;
        LoadSector(currentSector);

        bool running = true;
        while (running)
        {
            Console.SetCursorPosition(0, 0);

            if (inCave)
            {
                if (lightingTimer < 5)
                {
                    lightingTimer++; // ISSUE: It only activates when you move.
                }
                else
                {
                    lightingTimer = 0;
                }
                if (lightingTimer == 0)
                {
                    if (lightingLevel1 > 232)
                    {
                        lightingLevel1--;
                    }
                    else if (lightingLevel1 == 232)
                    {
                        lightingLevel1 = 0;
                        Console.Clear();
                        Thread.Sleep(3000);
                        AnimateTextForMapWriter("El: Oh well, it looks like this is the end for us. We're stuck in a cave. No light. No hope. I will use my powers now to start this all over again from the first time we got in this weird world. See you on the other side. *You hear her struggle as she uses her powers to somehow reverse time. You black out - actually you couldn't see anyway.", 55);
                        Console.ReadLine();
                        Environment.Exit(0);
                    }

                    if (lightingLevel2 > 232)
                    {
                        lightingLevel2--;
                    }
                    else if (lightingLevel2 == 232)
                    {
                        lightingLevel2 = 0;
                    }

                    if (lightingLevel3 > 232)
                    {
                        lightingLevel3--;
                    }
                    else if (lightingLevel3 == 232)
                    {
                        lightingLevel3 = 0;
                    }

                    if (lightingLevel4 > 232)
                    {
                        lightingLevel4--;
                    }
                    else if (lightingLevel4 == 232)
                    {
                        lightingLevel4 = 0;
                    }
                }
            }

            PrintMapV1();

            while (Console.KeyAvailable)
                Console.ReadKey(false); // skips previous input chars

            ConsoleKeyInfo key = Console.ReadKey(); // reads a char
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    if (cursorY == 0 && currentSector > 4)
                    {
                        ChangeSector(-5);
                        cursorY = 29;
                    }
                    else MoveCursor(0, -1);
                    break;
                case ConsoleKey.DownArrow:
                    if (cursorY == 29 && currentSector < 19)
                    {
                        ChangeSector(5);
                        cursorY = 0;
                    }
                    else MoveCursor(0, 1);
                    break;
                case ConsoleKey.LeftArrow:
                    if (cursorX == 0 && currentSector != 0 && currentSector != 5 && currentSector != 10 && currentSector != 15 && currentSector != 20)
                    {
                        ChangeSector(-1);
                        cursorX = 29;
                    }
                    else MoveCursor(-1, 0);
                    break;
                case ConsoleKey.RightArrow:
                    if (cursorX == 29 && currentSector != 4 && currentSector != 9 && currentSector != 14 && currentSector != 19 && currentSector != 24)
                    {
                        ChangeSector(1);
                        cursorX = 0;
                    }
                    else MoveCursor(1, 0);
                    break;
            }
            Console.WriteLine($"{cursorX}, {cursorY} ");
        }
    }

    static void MoveCursor(int dx, int dy)
    {
        cursorX = Math.Max(0, Math.Min(sectorSize - 1, cursorX + dx));
        cursorY = Math.Max(0, Math.Min(sectorSize - 1, cursorY + dy));
    }
    static void PrintMapV1()
    {
        for (int i = 0; i < sectorSize; i++) // Y
        {
            for (int j = 0; j < sectorSize; j++) // X
            {
                renderingLighting = false;

                ///////// NEED TO DETECT IF THE MAP POSITION IS RENDERING LIGHTING AND IT'S BEHIOND A WALL

                if (inCave && map[i, j] != 8 && map[i,j] != 9 && map[i,j] != 7 && !isLightBehindWall)
                {
                    if (i == cursorY + 1 && j == cursorX)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel1 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 2 && j == cursorX)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel2 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 3 && j == cursorX)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 4 && j == cursorX)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 1 && j == cursorX)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel1 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 2 && j == cursorX)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel2 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 3 && j == cursorX)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 4 && j == cursorX)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }

                    else if (i == cursorY && j == cursorX - 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel1 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY && j == cursorX - 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel2 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY && j == cursorX - 3)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY && j == cursorX - 4)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY && j == cursorX + 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel1 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY && j == cursorX + 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel2 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY && j == cursorX + 3)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY && j == cursorX + 4)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 1 && j == cursorX + 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel2 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 1 && j == cursorX + 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 1 && j == cursorX + 3)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 2  && j == cursorX + 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 2 && j == cursorX + 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 3 && j == cursorX + 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 1 && j == cursorX + 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel2 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 1 && j == cursorX + 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 1 && j == cursorX + 3)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 2 && j == cursorX + 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 2 && j == cursorX + 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 3 && j == cursorX + 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 3 && j == cursorX - 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 2 && j == cursorX - 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 2 && j == cursorX - 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 1 && j == cursorX - 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel2 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 1 && j == cursorX - 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY - 1 && j == cursorX - 3)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 1 && j == cursorX - 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel2 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 1 && j == cursorX - 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 1 && j == cursorX - 3)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 2 && j == cursorX - 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel3 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 2 && j == cursorX - 2)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    else if (i == cursorY + 3 && j == cursorX - 1)
                    {
                        Console.Write("\x1b[48;5;" + lightingLevel4 + "m  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        renderingLighting = true;
                    }
                    // FINISH THIS
                }

                if (i == cursorY && j == cursorX)
                {
                    Console.BackgroundColor = ConsoleColor.Magenta; // Unique cursor color
                    Console.ForegroundColor = ConsoleColor.White;
                    if (map[i, j] == 5)
                    {
                        Console.Clear();
                        Console.WriteLine("Dead.");
                        Console.ReadLine();
                    }
                    if (map[i,j] == 9)
                    {
                        lightingLevel1 = lightingLevel1Default;
                        lightingLevel2 = lightingLevel2Default;
                        lightingLevel3 = lightingLevel3Default;
                        lightingLevel4 = lightingLevel4Default;
                    }
                    if (map[i, j] == 7)
                    {
                        if (inCave)
                        {
                            Console.Clear();
                            AnimateTextForMapWriter("You try to exit the way you entered. A trapdoor opens below you, and you black out. You are back where you started.", 55);
                            Console.ReadLine();
                        }
                        else
                        {
                            inCave = true;
                        }
                    }
                    if (map[i,j] == 8)
                    {
                        Console.Clear();
                        AnimateTextForMapWriter("You fall down a hole. You black out. You are back where you started.", 55);
                        Console.ReadLine();
                    }
                }
                else
                {
                    Console.BackgroundColor = GetColor(map[i, j]);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                //Console.Write(map[i, j] < 10 ? $" {map[i, j]}|" : $"{map[i, j]}|");

                if (renderingLighting == false)
                {
                    Console.Write("  ");
                }
                Console.ResetColor();
            }
            Console.WriteLine();
        }
        Console.WriteLine($"Current Sector: {currentSector}                      ");
    }

    static ConsoleColor GetColor(int tileType)
    {
        switch (tileType)
        {
            case 0: return ConsoleColor.Black; // Empty tile
            case 1: return ConsoleColor.Gray;  // Path
            case 2: return ConsoleColor.Yellow;   // House
            case 3: return ConsoleColor.Blue;  // Water
            case 4: return ConsoleColor.DarkGreen; // Forest
            case 5: return ConsoleColor.Red; // Wall
            case 6: return ConsoleColor.Yellow; // Forest Special
            case 7: return ConsoleColor.Black; // Cave Entrance
            case 8: return ConsoleColor.Black; // Cave Drop
            case 9: return ConsoleColor.Yellow; // Torch Refill
            default: return ConsoleColor.Black; // Undefined
        }
    }

    static void ChangeSector(int increment)
    {
        currentSector += increment;
        LoadSector(currentSector);
    }

    static void LoadSector(int sectorNumber)
    {
        string filename = $"C:\\Users\\kianc\\source\\repos\\Map Editor V1.3\\Map Editor V1.3\\bin\\Debug\\sector{sectorNumber + 1}.txt";
        if (File.Exists(filename))
        {
            string[] lines = File.ReadAllLines(filename);
            for (int i = 0; i < sectorSize; i++)
            {
                string[] tokens = lines[i].Trim().Split(' ');
                for (int j = 0; j < sectorSize; j++)
                {
                    map[i, j] = int.Parse(tokens[j]);
                }
            }
        }
        else
        {
            Console.Clear();
            Console.WriteLine("No map file found");
            Console.WriteLine($"Attempted file: {filename}");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
    static void AnimateTextForMapWriter(string text, int delay)
    {
        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep(delay); // Adjusted for faster animation
        }
        Console.WriteLine();
    }
}