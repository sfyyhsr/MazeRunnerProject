using System;
using System.Collections.Generic;

namespace MazeRunnerProject
{
    public class Player : IAttackable
    {
        public string Name { get; set; }
        public int Health { get; set; } = 100;
        public int Score { get; set; }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            Console.WriteLine($"!!! {Name} took {amount} damage! Current Health: {Health} !!!");
        }

        public void AddScore(int points)
        {
            Score += points;
            Console.WriteLine($"Score +{points}! Total: {Score}");
        }
    }
}