namespace MazeRunnerProject
{
    // Matches <<Interface>> IAttackable in UML
    public interface IAttackable
    {
        void TakeDamage(int amount);
    }

    // Matches the "collects" relationship in UML
    public interface ICollectable
    {
        void Collect();
    }
}