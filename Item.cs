using System;
namespace MazeRunnerProject
{
    // Base Item class
    public abstract class Item : ICollectable
    {
        public string Name { get; set; }
        public abstract void Collect(Player player);
        public void Collect() { } // satisfies interface
    }

    public class Potion : Item
    {
        public int HealAmount { get; set; }

        public Potion(int healAmount)
        {
            Name = "Health Potion";
            HealAmount = healAmount;
        }

        public override void Collect(Player player)
        {
            player.Health = Math.Min(100, player.Health + HealAmount);
            Console.WriteLine($"  [POTION] +{HealAmount} HP restored! ({player.Health}/100)");
        }
    }

    public class Key : Item
    {
        public Key()
        {
            Name = "Maze Key";
        }

        public override void Collect(Player player)
        {
            Console.WriteLine("  [KEY] You found the KEY! Reach the EXIT (E).");
        }
    }
}