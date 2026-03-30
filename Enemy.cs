using System;

namespace MazeRunnerProject
{
    // The Base Class
    public abstract class Enemy : IAttackable
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int Damage { get; set; }

        public abstract void Attack(IAttackable target);

        public void TakeDamage(int amount)
        {
            Health -= amount;
            Console.WriteLine($"{Name} hit! Health: {Health}");
        }
    }

    public class BossEnemy : Enemy
    {
        public override void Attack(IAttackable target)
        {
            Console.WriteLine($"[BOSS] {Name} uses an Area Attack!");
            target.TakeDamage(this.Damage + 15);
        }
    }

    public class FastEnemy : Enemy
    {
        public override void Attack(IAttackable target)
        {
            Console.WriteLine($"{Name} strikes twice quickly!");
            target.TakeDamage(this.Damage);
        }
    }
}