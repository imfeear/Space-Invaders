using System.Collections.ObjectModel;
using System.Linq;
using SpaceInvaders.Models;

namespace SpaceInvaders.ViewModels;

public class GameViewModel
{
    public Player Player { get; set; }
    public ObservableCollection<Enemy> Enemies { get; set; }
    public ObservableCollection<Bullet> Bullets { get; set; }
    public GameState State { get; set; }

    private bool moveLeft, moveRight;
    private double enemyDirection = 5; // Direção inicial dos inimigos
    private bool enemiesShouldDescend = false; // Marca se os inimigos devem descer

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

        // Teste: Atualizar o Score após 2 segundos
        Task.Delay(2000).ContinueWith(_ =>
        {
            State.Score = 100;
            State.Lives = 2;
        });
        SpawnEnemies();
    }

    public void UpdateGame()
    {
        // Atualiza movimentação do jogador
        if (moveLeft && Player.X > 0)
            Player.MoveLeft();
        if (moveRight && Player.X < 760) // Limite da tela
            Player.MoveRight();

        // Atualiza movimentação dos tiros
        foreach (var bullet in Bullets.ToList())
        {
            bullet.Move(); // Atualiza a posição vertical do projétil

            // Remove o projétil se sair da tela
            if (bullet.Y < 0 || bullet.Y > 600)
            {
                Bullets.Remove(bullet);
            }
        }

        // Atualiza movimentação dos inimigos
        foreach (var enemy in Enemies)
        {
            enemy.X += enemyDirection;

            // Detecta bordas da tela
            if (enemy.X < 0 || enemy.X > 760)
            {
                enemiesShouldDescend = true;
                break;
            }
        }

        if (enemiesShouldDescend)
        {
            foreach (var enemy in Enemies)
            {
                enemy.Y += 20; // Move inimigos para baixo
            }

            enemyDirection *= -1; // Inverta a direção dos inimigos
            enemiesShouldDescend = false;
        }

        // Verifica colisões
        CheckCollisions();

        // Verifica condições de fim de jogo
        CheckGameOver();
    }

    public void MovePlayerLeft() => moveLeft = true;
    public void MovePlayerRight() => moveRight = true;
    public void StopMovingLeft() => moveLeft = false;
    public void StopMovingRight() => moveRight = false;

    public void PlayerShoot()
    {
        var bullet = Player.Shoot();
        Bullets.Add(bullet);
    }

    private void SpawnEnemies()
    {
        // Adiciona inimigos em uma grade
        for (int row = 0; row < 1; row++)
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
            foreach (var enemy in Enemies.ToList())
            {
                // Verifica colisão entre projéteis e inimigos
                if (bullet.X >= enemy.X && bullet.X <= enemy.X + 40 &&
                    bullet.Y >= enemy.Y && bullet.Y <= enemy.Y + 40)
                {
                    Enemies.Remove(enemy);
                    Bullets.Remove(bullet);
                    State.Score += 100; // Incrementa a pontuação
                    break;
                }
            }
        }
    }

    private void CheckGameOver()
    {
        // Verifica se o jogador perdeu todas as vidas
        if (State.Lives <= 0)
        {
            State.IsGameOver = true;
        }

        // Verifica se todos os inimigos foram destruídos
        if (Enemies.Count == 0)
        {
            State.IsGameOver = true;
        }
    }
}
