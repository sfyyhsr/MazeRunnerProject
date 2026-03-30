using System;
using MazeRunnerProject;

namespace MazeRunnerProject
{
    class Program
    {
        static void Main()
        {

            GameManager engine = new GameManager();
            engine.InitializeGame();

            Console.WriteLine("\nPress any key to close the window...");
            Console.ReadKey();
        }
    }
}