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
    
    
    
    public ObservableCollection<LeaderboardEntry> Leaderboard { get; set; }
    public ObservableCollection<Boss> Bosses { get; set; }

    private bool moveLeft, moveRight;
    private double enemyDirection = 1; // Direção inicial dos inimigos
    private bool enemiesShouldDescend = false; // Marca se os inimigos devem descer
    private int moveDownCounter = 0;
    private Random random = new Random();
    private double bossDirection = 1;
    public ObservableCollection<Shield> Shields { get; set; }

    public GameViewModel()
    {
        Player = new Player(400, 500);
        Enemies = new ObservableCollection<Enemy>();
        Bullets = new ObservableCollection<Bullet>();
        Shields = new ObservableCollection<Shield>();
        Bosses = new ObservableCollection<Boss>();  // Inicializa a lista de Bosses
        State = new GameState
        {
            Score = 0,
            Lives = 3,
            IsGameOver = false
        };
        
        
        SpawnEnemies();
        SpawnShields();
        
        Leaderboard = new ObservableCollection<LeaderboardEntry>(LeaderboardManager.LeaderboardEntries);
        
    }

    public void UpdateGame()
    {
        // Verifica se o jogador não ultrapassa a borda esquerda ou direita
        if (moveLeft && Player.X > 0)
            Player.MoveLeft();  // Só move se não ultrapassar a borda esquerda
        if (moveRight && Player.X < 800 - 40)  // Considerando o tamanho do jogador (40px)
            Player.MoveRight();  // Só move se não ultrapassar a borda direita

        // Atualiza todos os tiros
        foreach (var bullet in Bullets.ToList())
        {
            bullet.Move();  // O movimento agora será apenas vertical (Y)
            if (bullet.Y < 0 || bullet.Y > 600)  // Verifica se o tiro saiu da tela
                Bullets.Remove(bullet);  // Remove o tiro se ele for fora da tela
        }

        // Atualiza todos os inimigos
        foreach (var enemy in Enemies.ToList())
        {
            if (enemy.Y > 600)  // Se o inimigo ultrapassar a borda inferior
            {
                Enemies.Remove(enemy);  // Remove o inimigo que caiu para fora da tela
            }

            if (enemy.X < 0 || enemy.X > 800 - 40)  // Considerando o tamanho do inimigo (40px)
            {
                enemyDirection *= -1;  // Inverte a direção
                enemy.Y += 10;  // Faz o inimigo descer um pouco após atingir a borda
            }

            enemy.X += enemyDirection;

            // Verifica se o inimigo tocou o jogador (perde 2 vidas)
            if (enemy.X < Player.X + 40 && enemy.X + 40 > Player.X && enemy.Y < Player.Y + 20 && enemy.Y + 20 > Player.Y)
            {
                Enemies.Remove(enemy);  // Remove o inimigo que tocou o jogador
                State.Lives -= 2;  // O jogador perde 2 vidas ao colidir com um inimigo
                if (State.Lives <= 0)
                {
                    State.IsGameOver = true;  // Se o jogador não tiver mais vidas, o jogo termina
                }
            }
        }
        
        
        foreach (var boss in Bosses.ToList())
        {
            boss.X += bossDirection;

            // Verifica a fase do Boss e altera seu comportamento
            boss.CheckPhase();

            // Se o boss atingir as bordas, muda a direção e desce
            if (boss.X < 0 || boss.X > 800 - 60)
            {
                bossDirection *= -1;
                boss.Y += 10;  // Faz o boss descer um pouco após atingir a borda
            }

            // O Boss atira dependendo do seu modo de ataque
            if (boss.AttackMode == "Circular")
            {
                boss.CircularAttack(Bullets);  // Ativa o ataque especial circular
            }
            else
            {
                boss.Shoot(Bullets);  // Tiros normais (ou triplos)
            }

            // O Boss ataca o jogador
            boss.Attack(Player);
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

        EnemyShoot();
        CheckCollisions();
        CheckGameOver();
    }

    private void SpawnBoss(double xPosition,double yPosition)
    {
        // Coloca o boss mais para cima (pode ajustar o valor conforme necessário)
        Bosses.Add(new Boss(xPosition, yPosition));
    }

    public void MovePlayerLeft() => moveLeft = true;
    public void MovePlayerRight() => moveRight = true;
    public void StopMovingLeft() => moveLeft = false;
    public void StopMovingRight() => moveRight = false;

    public void PlayerShoot()
    {
        // Cria o tiro com a velocidade correta no eixo Y (para cima)
        var bullet = new Bullet(Player.X + 15, Player.Y - 10, -9, true);  // Tiro do jogador se movendo para cima
        Bullets.Add(bullet);
    }

    
    private void EnemyShoot()
    {
        foreach (var enemy in Enemies)
        {
            if (random.NextDouble() < 0.001)
            {
                var bullet = new Bullet(enemy.X + 15, enemy.Y + 10, 5, false);
                Bullets.Add(bullet);
            }
        }
    }

    private void SpawnEnemies(int additionalEnemies = 0)
    {
        int totalEnemies = 55 + additionalEnemies;  // 55 é o número inicial de inimigos, e additionalEnemies é o incremento

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 11; col++)
            {
                if (Enemies.Count < totalEnemies)
                {
                    // Ajuste a posição Y para mover os inimigos mais para baixo
                    double adjustedY = row * 50 + 100;  // Aqui, 100 é o valor que desloca os inimigos para baixo

                    Enemies.Add(new Enemy(col * 60 + 20, adjustedY));  // Adiciona os inimigos com a nova posição Y
                }
            }
        }
    }



    
    private void SpawnShields()
    {
        // Criando 3 barricadas na parte inferior da tela
        for (int i = 0; i < 4; i++)
        {
            Shields.Add(new Shield(i * 200 + 100, 400)); // Posiciona as barricadas
        }
    }

public void CheckCollisions()
{
    // Percorre a lista de tiros
    foreach (var bullet in Bullets.ToList())
    {
        // Tiros do jogador
        if (bullet.IsPlayerBullet)
        {
            // Verifica colisão com inimigos
            foreach (var enemy in Enemies.ToList())
            {
                if (bullet.X >= enemy.X && bullet.X <= enemy.X + 40 &&
                    bullet.Y >= enemy.Y && bullet.Y <= enemy.Y + 40)
                {
                    enemy.TakeDamage();  // Diminui a saúde do inimigo

                    if (enemy.Health <= 0)  // Se o inimigo morrer, remove-o da lista
                    {
                        Enemies.Remove(enemy);
                    }

                    Bullets.Remove(bullet);  // Remove o tiro ao atingir o inimigo
                    State.Score += 100;  // Aumenta a pontuação ao matar um inimigo
                    break;  // Sai da verificação de colisão com inimigos
                }
            }

            // Verifica colisão com o boss (somente os tiros do jogador)
            foreach (var boss in Bosses.ToList())
            {
                if (bullet.X >= boss.X && bullet.X <= boss.X + 60 &&
                    bullet.Y >= boss.Y && bullet.Y <= boss.Y + 60)
                {
                    boss.TakeDamage();  // Diminui a saúde do boss

                    if (boss.Health <= 0)  // Se o boss morrer, remove-o da lista
                    {
                        Bosses.Remove(boss);
                        State.Score += 500;  // Aumenta a pontuação ao matar o boss
                    }

                    Bullets.Remove(bullet);  // Remove o tiro ao atingir o boss
                    break;  // Sai da verificação de colisão com o boss
                }
            }
        }
        else // Tiros dos inimigos
        {
            // Verifica colisão com as barricadas
            foreach (var shield in Shields.ToList())
            {
                if (bullet.X >= shield.X && bullet.X <= shield.X + 40 &&
                    bullet.Y >= shield.Y && bullet.Y <= shield.Y + 20)
                {
                    shield.TakeDamage();  // Diminui a saúde da barricada

                    if (shield.Health <= 0)  // Se a barricada for destruída, remove-a
                    {
                        Shields.Remove(shield);
                    }

                    Bullets.Remove(bullet);  // Remove o tiro ao atingir a barricada
                    break;  // Sai da verificação de colisão com barricadas
                }
            }

            // Verifica colisão com o jogador
            if (bullet.X >= Player.X && bullet.X <= Player.X + 40 &&
                bullet.Y >= Player.Y && bullet.Y <= Player.Y + 20)
            {
                Bullets.Remove(bullet);  // Remove o tiro ao atingir o jogador
                State.Lives--;  // Perde uma vida
                if (State.Lives <= 0)
                {
                    State.IsGameOver = true;  // Se o jogador não tiver mais vidas, o jogo termina
                }
            }
        }
    }
    // Verifica se algum inimigo tocou no player (colisão física)
    foreach (var enemy in Enemies.ToList())
    {
        if (enemy.X >= Player.X && enemy.X <= Player.X + 40 &&
            enemy.Y >= Player.Y && enemy.Y <= Player.Y + 20)
        {
            Enemies.Remove(enemy);  // Remove o inimigo que tocou o jogador
            State.Lives -= 2;  // Perde 2 vidas ao colidir com o inimigo
            if (State.Lives <= 0)
            {
                State.IsGameOver = true;  // Se o jogador não tiver mais vidas, o jogo termina
            }
        }
    }
}



    

private void CheckGameOver()
{
    // Se o jogador perdeu todas as vidas
    if (State.Lives <= 0)
    {
        State.IsGameOver = true;
    }
    // Se o jogador ganhou, ou seja, todos os inimigos e o boss foram derrotados
    else if (!Enemies.Any() && !Bosses.Any())
    {
          // O jogo termina
        State.Score += 1000;  // Dá uma pontuação bônus pela vitória
        NextLevel();  // Chama o método para passar para a próxima fase
    }
}


public void NextLevel()
{
    // Aumenta a dificuldade aumentando o número de inimigos
    int additionalEnemies = State.Score / 1000; // A cada 1000 pontos, aumenta mais inimigos
    SpawnEnemies(additionalEnemies);  // Passa o número adicional de inimigos para SpawnEnemies

    // Aumenta a velocidade dos inimigos e do boss
    enemyDirection *= 1.1;  // Aumenta a velocidade dos inimigos
    bossDirection *= 1.1;   // Aumenta a velocidade do boss

    // Aumenta a saúde e o dano do boss
    foreach (var boss in Bosses)
    {
        boss.Health += 5;  // Aumenta a saúde do boss
        boss.BossDamage += 1;  // Aumenta o dano do boss
    }

    // Ajuste da posição para o boss aparecer acima dos inimigos
    double bossYPosition = 10;

    // Criação de bosses com base na fase
    if (State.Score >= 20000) // Fase 5
    {
        // Cria o primeiro boss
        SpawnBoss(400,bossYPosition);  // Primeiro boss

        // Cria o segundo boss um pouco ao lado do primeiro
        double secondBossXPosition = 400 + 150;  // Desloca o segundo boss para a direita
        SpawnBoss(secondBossXPosition, bossYPosition);  // Segundo boss posicionado ao lado
    }
    else if (State.Score >= 7000) // Fase 2, 3 e 4
    {
        SpawnBoss(400,bossYPosition); // Para as fases 2 a 4, cria apenas um boss
    }

    // Atualiza a dificuldade dos inimigos
    foreach (var enemy in Enemies)
    {
        enemy.Speed += 0.2;  // Aumenta a velocidade dos inimigos
    }

    // Restaura as barricadas preservadas
    var currentShields = Shields.ToList();
    Shields.Clear();
    foreach (var shield in currentShields)
    {
        Shields.Add(shield);  // Mantém as barricadas no estado atual
    }

    // Adiciona uma vida ao jogador ao passar de fase
    State.Lives += 1;

    // Reseta o cooldown do ataque especial do boss
    foreach (var boss in Bosses)
    {
        boss.ResetSpecialAttackCooldown();  // Reseta o cooldown do ataque especial do boss
    }
}






public void RestartGame()
{
    Player = new Player(400, 500);
    State.IsGameOver = false;
    Enemies.Clear();
    Bullets.Clear();
    Bosses.Clear();  // Remove o boss da tela
    Shields.Clear();  // Limpa as barricadas

    SpawnEnemies();
    SpawnShields();  // Recria as barricadas do começo
}

    
}