using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SpaceInvaders.Models;
using WpfAnimatedGif;
using System.Windows.Media.Imaging;

namespace SpaceInvaders.ViewModels;

public class GameViewModel
{
    private MediaPlayer shotSoundPlayer;
    private MainWindow _mainWindow;
    private MediaPlayer explosionSoundPlayer;
    private MediaPlayer supremeBossDefeatedSoundPlayer;



    public Player Player { get; set; }
    public ObservableCollection<Enemy> Enemies { get; set; }
    public ObservableCollection<Bullet> Bullets { get; set; }
    public GameState State { get; set; }
    
    private double lastShootTime = 0; // Armazena o tempo do último disparo
    private double shootCooldown = 0.2; // Intervalo de 0.5 segundos entre os tiros
    private readonly double normalShootCooldown = 0.2;
    private readonly double boostedShootCooldown = 0;
    
    private DateTime bonusStartTime;  // Para armazenar o tempo de início do bônus
    private bool isDoubleShootActive = false;  // Para verificar se o bônus de disparo duplo está ativo
    private double doubleShootDuration = 4; // Duração do bônus de tiro duplo em segundos
    private int nextDoubleShootThreshold = 2000;
    
    public bool supremeBossActive = false;
    private bool supremeBossDefeated = true;
    private int supremeBossDifficulty = 0;  // Nível de dificuldade do Boss Supremo
    private int nextSupremeBossThreshold = 50000;  // Primeira aparição aos 50.000 pontos

    private List<Enemy> savedEnemiesBeforeSupremeBoss = new List<Enemy>();
    
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
        explosionSoundPlayer = new MediaPlayer();
        try
        {
            explosionSoundPlayer.Open(new Uri("C:\\Users\\Bruno\\RiderProjects\\SpaceInvaders\\SpaceInvaders\\Assets\\explosion_sound.wav"));  // Caminho para o som de explosão no formato WAV
            explosionSoundPlayer.Volume = 0.6;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar o som de explosão: {ex.Message}");
        }
        
        supremeBossDefeatedSoundPlayer = new MediaPlayer();
        try
        {
            supremeBossDefeatedSoundPlayer.Open(new Uri("C:\\Users\\Bruno\\RiderProjects\\SpaceInvaders\\SpaceInvaders\\Assets\\supremeboss_killed.wav"));  // Caminho para o som de derrota do Boss Supremo no formato WAV
            supremeBossDefeatedSoundPlayer.Volume = 0.6;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar o som de derrota do Boss Supremo: {ex.Message}");
        }

        
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
        
        shotSoundPlayer = new MediaPlayer();
        try
        {
            shotSoundPlayer.Open(new Uri("C:\\Users\\Bruno\\RiderProjects\\SpaceInvaders\\SpaceInvaders\\Assets\\shot_sound.wav"));  // Caminho para o som de disparo no formato WAV
            shotSoundPlayer.Volume = 0.5;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar o som: {ex.Message}");
        }
        
        SpawnEnemies();
        SpawnShields();
        
        Leaderboard = new ObservableCollection<LeaderboardEntry>(LeaderboardManager.LeaderboardEntries);
        
    }
    
    private void PlayExplosionSound()
    {
        System.Diagnostics.Debug.WriteLine("Player morreu. Tocando som de explosão.");
        explosionSoundPlayer.Position = TimeSpan.Zero;
        explosionSoundPlayer.Play();
    }

    public void UpdateGame()
    {
        if (State.Lives <= 0 && !State.IsGameOver)
        {
            State.IsGameOver = true;
        }

        Console.WriteLine($"🟢 Verificação do Supreme Boss - Score: {State.Score}, Próximo Boss: {nextSupremeBossThreshold}, Boss Ativo: {supremeBossActive}, Boss Derrotado: {supremeBossDefeated}");

        // Ativa o bônus apenas ao atingir múltiplos de 2000 pontos
        if (State.Score >= nextDoubleShootThreshold && !isDoubleShootActive)
        {
            ActivateDoubleShootBonus();
            nextDoubleShootThreshold += 2000; // Define o próximo limiar para ativação
        }

        // Desativa o bônus após 10 segundos
        if (isDoubleShootActive && (DateTime.Now - bonusStartTime).TotalSeconds >= doubleShootDuration)
        {
            Console.WriteLine("Desativando o bônus de tiro duplo!");
            DeactivateDoubleShootBonus();
        }
        
        if (State.Score >= nextSupremeBossThreshold && !supremeBossActive && supremeBossDefeated)
        {
            ActivateSupremeBoss();
        }
        


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
            boss.CheckPhase();

            // Ajustando os limites de colisão conforme o tamanho do Supreme Boss
            int bossWidth = boss is SupremeBoss ? 110 : 60;
            int correctionOffset = 5; // Define o tamanho real do boss

            if (boss.X <= 0 || (boss.X + bossWidth) >= (800 - correctionOffset))
            {
                bossDirection *= -1;
                boss.Y += 10; // Move um pouco para baixo após atingir a borda
            }

            if (boss.AttackMode == "Circular")
            {
                boss.CircularAttack(Bullets);
            }
            else
            {
                boss.Shoot(Bullets);
            }

            boss.Attack(Player);

            if (boss is SupremeBoss supremeBoss)
            {
                supremeBoss.UpdateBoss(Bullets);
            }
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

    private void SpawnBoss(double xPosition, double yPosition)
    {
        if (!supremeBossActive) 
        {
            Console.WriteLine($"🟢 Criando boss normal em {xPosition}, {yPosition}");
            Bosses.Add(new Boss(xPosition, yPosition));
        }
    }

    private void ActivateSupremeBoss()
    {
        Console.WriteLine($"⚠️ Boss Supremo apareceu! Dificuldade: {supremeBossDifficulty}");

        supremeBossActive = true;
        supremeBossDefeated = false; // O jogador precisa derrotá-lo primeiro!
        supremeBossDifficulty++; // Ele fica mais difícil a cada aparição

        // Atualiza para o próximo spawn em 50k pontos a mais
        nextSupremeBossThreshold += 50000; 

        // Salvar os inimigos antes do boss aparecer
        savedEnemiesBeforeSupremeBoss = Enemies.ToList();

        // 🔴 GARANTE QUE NÃO TEM BOSS NORMAL ATIVO!
        Bosses.Clear();
        Enemies.Clear();
        
        Shields.Clear(); // Remove as barricadas antigas
        SpawnShields(); // Cria novas barricadas

        // Criar o Boss Supremo no meio da tela
        var supremeBoss = new SupremeBoss(350, 50, supremeBossDifficulty);
        Bosses.Add(supremeBoss);

        Console.WriteLine($"✅ Supreme Boss adicionado. Total de Bosses na lista: {Bosses.Count}");
    }

    


    public void MovePlayerLeft() => moveLeft = true;
    public void MovePlayerRight() => moveRight = true;
    public void StopMovingLeft() => moveLeft = false;
    public void StopMovingRight() => moveRight = false;

    public void PlayerShoot()
    {
        double currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond; // Tempo em segundos

        if (currentTime - lastShootTime >= shootCooldown)
        {
            var bullet1 = new Bullet(Player.X + 15, Player.Y - 10, -20, true);
            Bullets.Add(bullet1);

            if (isDoubleShootActive)
            {
                var bullet2 = new Bullet(Player.X - 15, Player.Y - 10, -20, true); // Disparo do outro lado do jogador
                Bullets.Add(bullet2);
            }
            
            Application.Current.Dispatcher.Invoke(() => {
                Console.WriteLine("Disparo! Reproduzindo som...");
                shotSoundPlayer.Stop();  // Para o som se estiver tocando
                shotSoundPlayer.Play();  // Toca o som
                shotSoundPlayer.Volume = 1.0;
            });

            lastShootTime = currentTime; // Atualiza o tempo do último disparo
        }
    }


    
    private void ActivateDoubleShootBonus()
    {
        isDoubleShootActive = true;
        bonusStartTime = DateTime.Now;
        shootCooldown = boostedShootCooldown;
        Console.WriteLine("Bônus de tiro duplo ativado!");
    }


    
    private void DeactivateDoubleShootBonus()
    {
        isDoubleShootActive = false;
        shootCooldown = normalShootCooldown;
        Console.WriteLine("Bônus de tiro duplo desativado!");
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

            // Verifica colisão com o boss (somente os tiros do jogador)// Verifica colisão com o boss (somente os tiros do jogador)
            foreach (var boss in Bosses.ToList())
            {
                if (bullet.X >= boss.X && bullet.X <= boss.X + 110 &&
                    bullet.Y >= boss.Y && bullet.Y <= boss.Y + 70)
                {
                    Console.WriteLine($"🔥 Boss atingido! Vida antes: {boss.Health}");

                    boss.TakeDamage();  // Diminui a saúde do boss

                    Console.WriteLine($"💥 Vida do boss agora: {boss.Health}");

                    if (boss.Health <= 0)  // Se o boss morreu
                    {
                        Console.WriteLine($"❌ O Boss foi derrotado!");

                        // Verifica se é um Supreme Boss antes de removê-lo
                        if (boss is SupremeBoss)
                        {
                            Console.WriteLine($"👑 Supreme Boss foi derrotado!");
                            supremeBossActive = false;
                            supremeBossDefeated = true;
                            State.Score += 10000; // Pontuação extra para o Supreme Boss
                            Console.WriteLine($"✅ Atualizando SupremeBossDefeated para {supremeBossDefeated}");
                            
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                supremeBossDefeatedSoundPlayer.Play();  // Toca o som de explosão
                            });
                            
                            // Espera 2 segundos para continuar e voltar ao jogo normal
                            Task.Delay(2000).ContinueWith(t =>
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    // Reverte o jogo para o estado normal
                                    Bosses.Remove(boss); // Remove o boss da lista
                                    supremeBossActive = false;
                                    supremeBossDefeated = true;
                                    State.Score += 10000;

                                    // Restaurar inimigos normais
                                    Enemies.Clear();
                                    foreach (var enemy in savedEnemiesBeforeSupremeBoss)
                                    {
                                        Enemies.Add(enemy);
                                    }

                                    // Aqui você pode adicionar mais lógica de reinicialização, se necessário
                                });
                            });
                        }
                        else
                        {
                            State.Score += 1000; // Pontuação para bosses normais
                        }

                        Bosses.Remove(boss); // Só remove o boss depois de atualizar os estados
                    }

                    Bullets.Remove(bullet);  // Remove o tiro ao atingir o boss
                    break;  // Sai da verificação de colisão com o boss
                }
            }

            
            foreach (var boss in Bosses.ToList())
            {
                Console.WriteLine($"🔍 Checando vida do Boss Supremo: {boss.Health}");
    
                if (boss.Health <= 0)
                {
                    Console.WriteLine($"❌ Supreme Boss DERROTADO! Próximo boss pode spawnar. Atualizando supremeBossDefeated para TRUE.");
                    
                    // Toca o som de explosão do Boss Supremo
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        supremeBossDefeatedSoundPlayer.Play();  // Toca o som de explosão
                    });
                    Task.Delay(2000).ContinueWith(t =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Remove o boss e continua o jogo normal
                            Bosses.Remove(boss);
                            supremeBossActive = false;
                            supremeBossDefeated = true;
                            State.Score += 10000;

                            Console.WriteLine($"✅ Boss Supremo derrotado! Ele voltará ao atingir {nextSupremeBossThreshold} pontos.");

                            // Restaurar inimigos normais
                            Enemies.Clear();
                            foreach (var enemy in savedEnemiesBeforeSupremeBoss)
                            {
                                Enemies.Add(enemy);
                            }

                            // Você pode restaurar mais elementos do jogo aqui, se necessário
                        });
                    });
                }
            
            }


            

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

        }
        // Tiros dos inimigos
        else
        {
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
        // Acesse a instância singleton da MainWindow
        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.ShowExplosionGif(Player.X, Player.Y);
        }
        else
        {
            Console.WriteLine("A MainWindow não está acessível.");
        }
        State.IsGameOver = true;
        PlayExplosionSound();

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
    if (!supremeBossActive) 
    {
        if (State.Score >= 20000) // Fase 5
        {
            SpawnBoss(400, bossYPosition);
            SpawnBoss(550, bossYPosition);
        }
        else if (State.Score >= 7000) // Fase 2, 3 e 4
        {
            SpawnBoss(400, bossYPosition);
        }
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
    // Resetando as variáveis do jogo
    Player = new Player(400, 500);
    State.IsGameOver = false;  // Marca que o jogo não está mais em Game Over
    State.Score = 0;  // Resetando a pontuação
    State.Lives = 3;  // Reinicia as vidas

    Enemies.Clear();
    Bullets.Clear();
    Bosses.Clear();  // Limpa qualquer boss da tela
    Shields.Clear();  // Limpa as barricadas

    // Reinicia o estado de bonus
    supremeBossActive = false;
    supremeBossDefeated = true;
    nextSupremeBossThreshold = 50000;

    // Inicializa o jogo com a fase 1
    SpawnEnemies();
    SpawnShields();  // Recria as barricadas do começo

    // Restabelece o estado do jogo
    nextDoubleShootThreshold = 2000;
    shootCooldown = normalShootCooldown;

    supremeBossActive = false;  // Resetando o estado do boss
    supremeBossDefeated = true; // Marcando o boss como derrotado para iniciar o próximo
    nextSupremeBossThreshold = 50000; // Resetando o limiar de pontos para o próximo Supremo Boss
}

    
}