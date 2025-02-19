using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SpaceInvaders.Models;

namespace SpaceInvaders.ViewModels;

public class GameViewModel
{
    public Player Player { get; set; }
    public ObservableCollection<Enemy> Enemies { get; set; }
    public ObservableCollection<Bullet> Bullets { get; set; }
    public GameState State { get; set; }

    private bool moveLeft, moveRight;
    private double enemyDirection = 1; // Direção inicial dos inimigos
    private bool enemiesShouldDescend = false; // Marca se os inimigos devem descer
    private int moveDownCounter = 0;
    private Random random = new Random();

    public GameViewModel()
    {
        Player = new Player(400, 500);
        Enemies = new ObservableCollection<Enemy>();
        Bullets = new ObservableCollection<Bullet>();
        State = new GameState
        {
            Score = 0,
            Lives = 3,
            IsGameOver = false
        };

        SpawnEnemies();
    }

    public void UpdateGame()
    {
        if (moveLeft)
            Player.MoveLeft();
        if (moveRight)
            Player.MoveRight();

        foreach (var bullet in Bullets.ToList())
        {
            bullet.Move();
            if (bullet.Y < 0 || bullet.Y > 600)
                Bullets.Remove(bullet);
        }

        moveDownCounter++;
        if (moveDownCounter % 120 == 0)
        {
            foreach (var enemy in Enemies)
            {
                enemy.Y += 10;
            }
            enemyDirection *= -1;
        }

        foreach (var enemy in Enemies)
        {
            enemy.X += enemyDirection;

            if (Enemies.Any(e => e.X <= 0) || Enemies.Any(e => e.X >= 760))
            {
                enemiesShouldDescend = true;
                break;
            }
        }

        if (enemiesShouldDescend)
        {
            foreach (var enemy in Enemies)
            {
                enemy.Y += 20;
            }
            enemyDirection *= -1;
            enemiesShouldDescend = false;
        }

        EnemyShoot();
        CheckCollisions();
        CheckGameOver();
    }

    public void MovePlayerLeft() => moveLeft = true;
    public void MovePlayerRight() => moveRight = true;
    public void StopMovingLeft() => moveLeft = false;
    public void StopMovingRight() => moveRight = false;

    public void PlayerShoot()
    {
        var bullet = new Bullet(Player.X + 15, Player.Y - 10, -5);
        Bullets.Add(bullet);
    }

    private void EnemyShoot()
    {
        foreach (var enemy in Enemies)
        {
            if (random.NextDouble() < 0.001)
            {
                var bullet = new Bullet(enemy.X + 15, enemy.Y + 10, 5);
                Bullets.Add(bullet);
            }
        }
    }

    private void SpawnEnemies()
    {
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 11; col++)
            {
                Enemies.Add(new Enemy(col * 60 + 20, row * 50));
            }
        }
    }

    private void CheckCollisions()
    {
        foreach (var bullet in Bullets.ToList())
        {
            if (bullet.Speed < 0)
            {
                foreach (var enemy in Enemies.ToList())
                {
                    if (bullet.X >= enemy.X && bullet.X <= enemy.X + 40 &&
                        bullet.Y >= enemy.Y && bullet.Y <= enemy.Y + 40)
                    {
                        Enemies.Remove(enemy);
                        Bullets.Remove(bullet);
                        State.Score += 100;
                        break;
                    }
                }
            }
            else
            {
                if (bullet.X >= Player.X && bullet.X <= Player.X + 40 &&
                    bullet.Y >= Player.Y && bullet.Y <= Player.Y + 20)
                {
                    Bullets.Remove(bullet);
                    State.Lives--;
                    if (State.Lives <= 0)
                    {
                        State.IsGameOver = true;
                    }
                }
            }
        }

        foreach (var enemy in Enemies)
        {
            if (enemy.X >= Player.X && enemy.X <= Player.X + 40 &&
                enemy.Y >= Player.Y && enemy.Y <= Player.Y + 20)
            {
                State.IsGameOver = true;
                break;
            }
        }
    }

    private void CheckGameOver()
    {
        if (State.Lives <= 0)
        {
            State.IsGameOver = true;
        }
        else if (!Enemies.Any()) // Se todos os inimigos forem derrotados
        {
            State.Lives++; // Concede uma vida extra
            RestartGame();
        }
    }

    public void RestartGame()
    {
        Player = new Player(400, 500);
        State.IsGameOver = false;
        Enemies.Clear();
        Bullets.Clear();
        SpawnEnemies();
    }
}