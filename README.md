# Project Tittle: Maze Runner

**Group Members:**

1. Mariha binti Masri - 24005602
2. Qurratu Nur Fayyadhah Binti Ahmad Mawaridi - 24006080
3. ⁠Medina Syadiya binti Mohamad Shahid - 24005634
4. ⁠Myazatul Sofea binti Azmi - 24006439
5. ⁠’Afifah Syakirah binti Ahmad Shamil - 24005854
6. ⁠Nur Hanani binti Hazlan -24005607

**Project Description:** 
This project focuses on the design and development of a simulation based game called Maze Runner, implemented using Object Oriented Programming principles in C#. The purpose of this project is to apply core OOP concepts such as encapsulation, inheritance, and polymorphism in building an interactive and structured game system. The game simulates a maze environment and allows the player to interact through controlled movements and actions.

The Maze Runner game is inspired by 2D grid-based maze games where the player navigates from a starting point to an exit, avoiding enemies or traps and collecting items. This type of simulation based game is suitable for OOP implementation because it involves multiple interacting entities like players and enemies. 

**System Features:**
1. Player Movement:
   - Navigate the maze using W, A, S, D or arrow keys.
2. Enemy AI:
   - Enemies patrol the maze and chase the player when alerted.
   - Two enemy types: Fast enemies (quick attacks) and Boss enemies (high damage).
3. Combat:
   - Attack nearby enemies using F or Q.
   - Critical hits, damage range, and score system implemented.
4. Collectibles:
   - Health potions (restore HP).
   - Keys (required to unlock exit).
5. Traps:
   - Damage the player if stepped on.
6. Level Progression:
   - Multiple levels with increasing difficulty.
   - Each level has unique layout and enemy spawn points.
7. Visibility & Fog of War:
   - Player can only see a certain radius around them.
8. Score System:
   - Points awarded for defeating enemies, collecting items, and completing levels.
9. User Interface:
   - Console-based HUD showing player HP, score, keys, and combat logs.
10. Pause & Exit:
    - Press Esc to quit the game at any time.

**OOP Concepts Used:**

1. Encapsulation:
   - Player, Enemy, and Item classes hide their internal data using properties and methods.
   - Example: Player’s Health and Score can only be changed via TakeDamage() or AddScore() methods, not directly.

2. Inheritance:
   - Enemy class is the base class.
   - BossEnemy and FastEnemy inherit from Enemy, sharing common attributes and behavior but can override attacks.
   - Item class is the base class, with Potion and Key inheriting from it.

3. Polymorphism:
   - Methods like Attack() and Collect() are implemented differently in derived classes.
   - GameController can handle all types of enemies or items through their base class references without knowing the exact type.

**Instructions to Run the Program:**

1. Make sure you have Visual Studio (or any C# IDE) installed.
2. Clone or download the project from GitHub:
   git clone <your-repo-link>
3. Open the solution file (.sln) in Visual Studio.
4. Build the project (Build -> Build Solution or press Ctrl+Shift+B).
5. Run the project (Debug -> Start Without Debugging or press Ctrl+F5).
6. Follow the on-screen instructions:
   - Move the player using W A S D keys.
   - Attack enemies using F or Q.
   - Collect items (Potions, Key) to progress.
   - Reach the exit (E) after collecting the key to advance levels.

**Project File Structure:**

MazeRunnerProject

GameController.cs - The main game logic: movement, attacks, enemy AI, level loading.
Player.cs - Player class with health, score, and damage handling.
Enemy.cs - Enemy base class and derived classes.
Item.cs - Base Item class and derived classes.
Level.cs - Level settings, visibility, and enemy spawn information.
Program.cs - Entry point of the program, runs GameControll. 
README.txt - This README file containing instructions and project details.

