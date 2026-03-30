using MazeRunnerProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class EnemyInstance
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Type { get; set; }  // "Fast" | "Boss"
    public Enemy EnemyData { get; set; }
    public bool IsAlerted { get; set; }
    public int PatrolDir { get; set; } = 1;
    public int StunTurns { get; set; }
}

public class GameManager
{

    private Player player;
    private int[,] currentMaze;
    private int playerX, playerY;
    private double visibilityRadius = 6.0;
    private List<EnemyInstance> enemies = new List<EnemyInstance>();
    private bool gameRunning = true;
    private bool hasKey = false;
    private int currentLevel = 1;
    private const int MAX_LEVELS = 2;

    private const int ATTACK_RANGE = 1;    
    private const int MIN_DAMAGE = 6;
    private const int MAX_DAMAGE = 12;
    private const double CRIT_CHANCE = 0.15;
    private Random rng = new Random();

    // threading
    private System.Timers.Timer enemyTimer;
    private readonly object mazeLock = new object();

    // Display Messages 
    private string statusMsg = "";
    private string combatLog = "";

    //  0=Path 1=Wall 2=Exit 3=Potion 4=Key 5=Trap (6 is ignored/treated as floor)
    private static readonly int[][,] AllMazes = new int[][,]
    {
        // Level 1
        new int[,]
        {
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
            { 1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1 },
            { 1,0,1,0,1,0,1,1,1,1,1,0,1,1,0,0,1 },
            { 1,0,1,0,0,0,0,3,0,0,1,0,1,0,0,0,1 },
            { 1,0,1,1,1,1,1,0,1,0,1,0,1,0,1,0,1 },
            { 1,0,0,0,0,0,1,0,1,0,0,0,0,0,1,0,1 },
            { 1,1,1,1,1,0,1,0,1,1,1,1,1,0,1,0,1 },
            { 1,6,0,0,1,0,0,0,0,0,0,0,1,0,1,0,1 },
            { 1,0,1,0,1,1,1,1,1,1,1,0,1,0,0,0,1 },
            { 1,0,1,0,0,0,0,5,0,0,1,0,0,0,1,0,1 },
            { 1,0,1,1,1,1,1,1,1,0,1,1,1,0,1,0,1 },
            { 1,4,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1 },
            { 1,0,1,1,1,1,1,1,1,1,1,1,1,0,1,0,1 },
            { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,1 },
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
        },

        // Level 2
        new int[,]
        {
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
            { 1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1 },
            { 1,0,1,1,1,0,1,0,1,1,1,1,1,1,1,1,1,0,1 },
            { 1,0,1,3,0,0,1,0,0,0,0,0,0,0,0,0,1,0,1 },
            { 1,0,1,1,1,1,1,1,1,1,1,0,1,1,1,0,1,0,1 },
            { 1,0,0,0,0,0,0,0,0,0,1,0,1,5,0,0,1,0,1 },
            { 1,1,1,1,1,1,1,1,1,0,1,0,1,1,1,1,1,0,1 },
            { 1,0,0,0,0,5,0,0,1,0,0,0,0,0,0,0,1,0,1 },
            { 1,0,1,1,1,1,1,0,1,1,1,1,1,1,1,0,1,0,1 },
            { 1,0,1,0,6,0,1,0,0,0,0,0,0,0,1,0,0,0,1 },
            { 1,0,1,0,1,0,1,1,1,1,1,1,1,0,1,1,1,0,1 },
            { 1,0,1,0,1,0,0,0,0,0,0,0,1,0,0,0,1,0,1 },
            { 1,0,1,0,1,1,1,1,1,1,1,0,1,1,1,0,1,0,1 },
            { 1,4,0,0,0,0,0,0,5,0,1,0,0,0,0,0,1,0,1 },
            { 1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,0,1 },
            { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,1 },
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
        }
    };

    private static readonly (int x, int y, string type)[][] LevelEnemySpawns =
    {
        new[] { (10, 3, "Fast"), (6, 9, "Fast") },
        new[] { (12, 3, "Fast"), (15, 7, "Fast"), (5, 11, "Boss") },
    };

    
    public void InitializeGame()
    {
        Console.CursorVisible = false;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        player = new Player { Name = "Hero" };
        ShowTitleScreen();
        LoadLevel(currentLevel);
        RunGameLoop();
    }
  private void LoadLevel(int level)
    {
        if (level > MAX_LEVELS)
            return;

        // Use Level class instead of plain if statements
        Level currentLevelData = new Level(level);
        currentLevelData.DisplayLevelIntro();

        currentMaze = (int[,])AllMazes[level - 1].Clone();
        hasKey = false;
        playerX = 1; playerY = 1;
        visibilityRadius = currentLevelData.VisibilityRadius;

        enemies.Clear();

        foreach (var (x, y, type) in LevelEnemySpawns[level - 1])
        {
            Enemy enemy = type == "Boss"
                ? new BossEnemy { Name = "Gorgon", Health = 80, Damage = 18 }
                : new FastEnemy { Name = "Shade", Health = 30, Damage = 10 };

            enemies.Add(new EnemyInstance
            {
                X = x,
                Y = y,
                Type = type,
                EnemyData = enemy
            });
        }

        StartEnemyTimer(currentLevelData.EnemyTimerInterval);
    }

    private void StartEnemyTimer(int interval)
    {
        enemyTimer?.Stop();
        enemyTimer?.Dispose();
        enemyTimer = new System.Timers.Timer(interval);
        enemyTimer.Elapsed += (s, e) => { lock (mazeLock) { MoveEnemies(); } };
        enemyTimer.AutoReset = true;
        enemyTimer.Start();
    }

    private void RunGameLoop()
    {
        while (gameRunning)
        {
            lock (mazeLock) { RenderFrame(); }

            if (!Console.KeyAvailable)
            {
                Thread.Sleep(25);
                continue;
            }

            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Escape)
            {
                gameRunning = false;
                break;
            }

            lock (mazeLock)
            {
                bool acted = HandleInput(key);
                if (acted) CheckTileEvents();
            }
        }

        enemyTimer?.Stop();
        enemyTimer?.Dispose();

        Thread.Sleep(200);
        ShowEndScreen();
    }

    private bool HandleInput(ConsoleKeyInfo key)
    {
        try
        {
            if (key.Key == ConsoleKey.F || key.Key == ConsoleKey.Q)
            {
                AttackNearbyEnemy();
                return true;
            }

            int px = playerX, py = playerY;
            if (key.Key == ConsoleKey.W || key.Key == ConsoleKey.UpArrow) py--;
            if (key.Key == ConsoleKey.S || key.Key == ConsoleKey.DownArrow) py++;
            if (key.Key == ConsoleKey.A || key.Key == ConsoleKey.LeftArrow) px--;
            if (key.Key == ConsoleKey.D || key.Key == ConsoleKey.RightArrow) px++;

            if (px == playerX && py == playerY) return false;

            var bump = enemies.FirstOrDefault(e => e.X == px && e.Y == py);
            if (bump != null) { DealDamageToEnemy(bump); return true; }

            if (IsWalkable(px, py)) { playerX = px; playerY = py; return true; }

            return false;
        }
        catch (Exception ex)
        {
            SetStatus($"  [ERROR] Invalid action: {ex.Message}");
            return false;
        }
    }

    private void AttackNearbyEnemy()
    {
        var targets = enemies
            .Where(e => Math.Abs(e.X - playerX) + Math.Abs(e.Y - playerY) <= ATTACK_RANGE)
            .ToList();

        if (!targets.Any())
        {
            SetStatus(" No enemy in range! Move closer to attack.");
            return;
        }

        var target = targets
            .OrderBy(e => Math.Abs(e.X - playerX) + Math.Abs(e.Y - playerY))
            .First();

        DealDamageToEnemy(target);
    }

    private void DealDamageToEnemy(EnemyInstance target)
    {
        int dmg = rng.Next(MIN_DAMAGE, MAX_DAMAGE + 1);
        bool crit = rng.NextDouble() < CRIT_CHANCE;
        if (crit) dmg = (int)(dmg * 1.75);
        if (crit && target.Type == "Boss") target.StunTurns = 1;

        target.EnemyData.TakeDamage(dmg);
        string tag = crit ? " *** CRITICAL! ***" : "";
        SetCombatLog($"  Hit {target.EnemyData.Name} for {dmg} dmg!{tag}  Enemy HP:{target.EnemyData.Health}");
        player.AddScore(dmg);

        if (target.EnemyData.Health <= 0)
        {
            int bonus = target.Type == "Boss" ? 150 : 50;
            player.AddScore(bonus);
            enemies.Remove(target);
            SetStatus($"  [KILL] {target.EnemyData.Name} slain! +{bonus} score");
        }
    }

    private void CheckTileEvents()
    {
        try
        {
            int tile = currentMaze[playerY, playerX];

            if (tile == 3)
            {
                Potion potion = new Potion(30 + currentLevel * 5);
                potion.Collect(player);
                currentMaze[playerY, playerX] = 0;
                SetStatus($"  [POTION] +{potion.HealAmount} HP restored! ({player.Health}/100)");
                player.AddScore(10);
            }

            if (tile == 4)
            {
                Key key = new Key();
                key.Collect(player);
                hasKey = true;
                currentMaze[playerY, playerX] = 0;
                SetStatus("  [KEY] You found the KEY! Reach the EXIT (E).");
                player.AddScore(20);
            }

            if (tile == 5)
            {
                int dmg = 12 + currentLevel * 3;
                player.TakeDamage(dmg);
                SetStatus($"  [TRAP] Triggered! -{dmg} HP. Remaining: {player.Health}");
                if (player.Health <= 0) { gameRunning = false; return; }
                currentMaze[playerY, playerX] = 0;
            }

            if (tile == 6)
            {
                currentMaze[playerY, playerX] = 0;
            }

            if (tile == 2)
            {
                if (!hasKey) { SetStatus("  [EXIT] Locked! Find the KEY (K) first."); return; }
                if (!gameRunning) return;
                player.AddScore(200 * currentLevel);
                enemyTimer?.Stop();

                currentLevel++;
                if (currentLevel > MAX_LEVELS)
                {
                    gameRunning = false; return;
                }

                Console.Clear();
                LoadLevel(currentLevel);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"  [ERROR] Tile event error: {ex.Message}");
        }
    }
    private void MoveEnemies()
    {
        if (!gameRunning) return;

        foreach (var enemy in enemies.ToList())
        {
            if (enemy.StunTurns > 0) { enemy.StunTurns--; continue; }

            double dist = Dist(enemy.X, enemy.Y, playerX, playerY);
            double alertRange = enemy.Type == "Boss" ? 999 : 6;

            if (dist <= alertRange) enemy.IsAlerted = true;

            if (!enemy.IsAlerted)
            {
                int wx = enemy.X + enemy.PatrolDir;
                if (IsWalkable(wx, enemy.Y) && !enemies.Any(e => e != enemy && e.X == wx && e.Y == enemy.Y))
                    enemy.X = wx;
                else
                    enemy.PatrolDir *= -1;
                continue;
            }

            // BOSS
            if (enemy.Type == "Boss" && dist <= 1.5)
            {
                int dmg = enemy.EnemyData.Damage + rng.Next(0, 10);
                player.TakeDamage(dmg);
                SetCombatLog($"  [BOSS] Gorgon STOMPS you for {dmg} dmg!  HP:{player.Health}");
                if (player.Health <= 0) { gameRunning = false; return; }
            }

            // MOVEMENT 
            var best = BestMoveToward(enemy, playerX, playerY);
            if (best.HasValue && !enemies.Any(e => e != enemy && e.X == best.Value.x && e.Y == best.Value.y))
            {
                enemy.X = best.Value.x;
                enemy.Y = best.Value.y;
            }

            // MELEE 
            if (enemy.X == playerX && enemy.Y == playerY && enemy.Type != "Boss")
            {
                enemy.EnemyData.Attack(player);
                SetCombatLog($"  [{enemy.EnemyData.Name}] Attacks you!  HP:{player.Health}");
                if (player.Health <= 0) { gameRunning = false; return; }
            }
        }
    }

    private (int x, int y)? BestMoveToward(EnemyInstance enemy, int tx, int ty)
    {
        var dirs = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
        (int x, int y)? best = null;
        double bestDist = Dist(enemy.X, enemy.Y, tx, ty);

        foreach (var (dx, dy) in dirs)
        {
            int nx = enemy.X + dx, ny = enemy.Y + dy;
            if (!IsWalkable(nx, ny)) continue;
            double d = Dist(nx, ny, tx, ty);
            if (d < bestDist) { bestDist = d; best = (nx, ny); }
        }

        if (!best.HasValue)
        {
            var shuffled = dirs.OrderBy(_ => rng.Next()).ToArray();
            foreach (var (dx, dy) in shuffled)
            {
                int nx = enemy.X + dx, ny = enemy.Y + dy;
                if (IsWalkable(nx, ny)) { best = (nx, ny); break; }
            }
        }
        return best;
    }

    private void RenderFrame()
    {
        Console.SetCursorPosition(0, 0);
        DrawHUD();
        DrawMaze();
        DrawControls();
    }

    private void DrawHUD()
    {
        int w = Math.Max((Console.WindowWidth > 0 ? Console.WindowWidth : 80) - 1, 60);

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"  Level {Math.Min(currentLevel, MAX_LEVELS)} of {MAX_LEVELS}       Score: {player.Score}".PadRight(w));

        int bars = (int)Math.Round(player.Health / 5.0);
        string hpBar = "HP  [" + new string('#', bars) + new string(' ', 20 - bars) + "]  " + player.Health + "/100";
        ConsoleColor hpColor = player.Health > 50 ? ConsoleColor.Green
                  : player.Health > 25 ? ConsoleColor.Yellow
                  : ConsoleColor.Red;
        Console.ForegroundColor = hpColor;
        Console.WriteLine(("  " + hpBar).PadRight(w));

        string keyStatus = hasKey ? "Key: GOT IT" : "Key: not found";
        Console.ForegroundColor = hasKey ? ConsoleColor.Yellow : ConsoleColor.DarkGray;
        Console.WriteLine(("  " + keyStatus).PadRight(w));

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(("  " + combatLog).PadRight(w));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(("  " + statusMsg).PadRight(w));

        Console.ResetColor();
        Console.WriteLine(new string('-', w));

        combatLog = "";
        statusMsg = "";
    }

    private void DrawMaze()
    {
        int rows = currentMaze.GetLength(0);
        int cols = currentMaze.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                double dist = Dist(x, y, playerX, playerY);

                if (dist > visibilityRadius)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  ");
                    continue;
                }

                bool dim = dist > visibilityRadius - 1.5;

                var enemy = enemies.FirstOrDefault(e => e.X == x && e.Y == y);

                if (y == playerY && x == playerX)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("@ ");
                }
                else if (enemy != null)
                {
                    Console.ForegroundColor = enemy.Type == "Boss" ? ConsoleColor.DarkRed : ConsoleColor.Red;
                    string sym = enemy.Type == "Boss" ? "B " : "f ";
                    Console.Write(sym);
                }
                else
                {
                    switch (currentMaze[y, x])
                    {
                        case 1: Console.ForegroundColor = dim ? ConsoleColor.DarkGray : ConsoleColor.Gray; Console.Write("# "); break;
                        case 2: Console.ForegroundColor = ConsoleColor.Yellow; Console.Write("E "); break;
                        case 3: Console.ForegroundColor = ConsoleColor.Magenta; Console.Write("H "); break;
                        case 4: Console.ForegroundColor = ConsoleColor.Yellow; Console.Write("K "); break;
                        case 5: Console.ForegroundColor = ConsoleColor.Red; Console.Write("^ "); break;
                        case 6: // old weapon tile -> show as floor
                        default:
                            Console.ForegroundColor = dim ? ConsoleColor.DarkGray : ConsoleColor.DarkGray;
                            Console.Write(". ");
                            break;
                    }
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private void DrawControls()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Move: WASD   Attack: F or Q   Quit: Esc");
        Console.ResetColor();
    }

    private void ShowTitleScreen()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine("  +---------------------------------------------------------+");
        Console.WriteLine("  |                  M A Z E   R U N N E R                  |");
        Console.WriteLine("  +---------------------------------------------------------+");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine("  How to play: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("    1. Find the KEY marked K on the map.");
        Console.WriteLine("    2. Go to the EXIT marked E to go to the next level.");
        Console.WriteLine("    3. Stay alive through all 2 levels to win!");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  Controls: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("    W A S D        Move around");
        Console.WriteLine("    F or Q         Attack");
        Console.WriteLine("    Esc            Quit");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  WATCH OUT FOR:");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("    f ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Shade  — runs toward you and attacks");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("    B ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Gorgon — big boss, hits very hard");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("  TIP: Pick up H (potion) to recover health. Avoid ^ traps!");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine();
        Console.WriteLine("  Press any key to begin...");
        Console.ResetColor();
        Console.ReadKey(true);
        Console.Clear();
    }

    private void ShowEndScreen()
    {
        Console.Clear();
        bool won = currentLevel > MAX_LEVELS || (currentLevel == MAX_LEVELS && player.Health > 0 && !gameRunning && currentMaze[playerY, playerX] == 2);

        Console.ForegroundColor = won ? ConsoleColor.Yellow : ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine("  +==========================================+");
        Console.WriteLine(won
            ? "  |      YOU CONQUERED THE MAZE RUNNER!      |"
            : "  |        DARKNESS CLAIMS YOUR SOUL...      |");
        Console.WriteLine("  +==========================================+");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\n  Final Score   : {player.Score}");
        Console.WriteLine($"  Health Left   : {Math.Max(0, player.Health)} / 100");
        Console.WriteLine($"  Level Reached : {Math.Min(currentLevel, MAX_LEVELS)} / {MAX_LEVELS}");
        Console.ResetColor();
    }

    private bool IsWalkable(int x, int y) =>
        y >= 0 && y < currentMaze.GetLength(0) &&
        x >= 0 && x < currentMaze.GetLength(1) &&
        currentMaze[y, x] != 1;

    private double Dist(int x1, int y1, int x2, int y2) =>
        Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

    private void SetStatus(string msg) => statusMsg = msg;
    private void SetCombatLog(string msg) => combatLog = msg;
}
