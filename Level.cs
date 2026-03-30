using System;
namespace MazeRunnerProject
{
    public class Level
    {
        public int LevelNumber { get; set; }
        public int EnemyTimerInterval { get; set; }
        public double VisibilityRadius { get; set; }
        public string Description { get; set; }

        public Level(int levelNumber)
        {
            LevelNumber = levelNumber;

            switch (levelNumber)
            {
                case 1:
                    EnemyTimerInterval = 1100;
                    VisibilityRadius = 6.4;
                    Description = "The Forgotten Halls";
                    break;
                case 2:
                    EnemyTimerInterval = 850;
                    VisibilityRadius = 6.8;
                    Description = "The Dark Depths";
                    break;
                default:
                    EnemyTimerInterval = 1000;
                    VisibilityRadius = 6.0;
                    Description = "Unknown";
                    break;
            }
        }

        public void DisplayLevelIntro()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n  === Level {LevelNumber}: {Description} ===\n");
            Console.ResetColor();
        }
    }
}