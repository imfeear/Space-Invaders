using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpaceInvaders.ViewModels;
using System.Windows.Threading;
using SpaceInvaders.Models;
using WpfAnimatedGif;

namespace SpaceInvaders
{
    public partial class MainWindow : Window
    {
        
        private MediaPlayer backgroundMusicPlayer;
        private GameViewModel gameViewModel;
        private string playerName;
        private DispatcherTimer gameTimer;
        private bool isScoreSaved = false;
        private MediaPlayer gameOverMusicPlayer;
        private DispatcherTimer endGameTimer;  // Timer para controlar o tempo de espera até exibir o painel de Game Over


        public static MainWindow Instance { get; private set; }
        
        public MainWindow(string playerName)
        {
            InitializeComponent();
            this.playerName = playerName;  // Armazena o nome do jogador
            gameViewModel = new GameViewModel();
            DataContext = gameViewModel;
            Instance = this;
            
            // Inicializa o timer do jogo para controlar a renderização
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(18) // Aproximadamente 60 FPS
            };
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            
            backgroundMusicPlayer = new MediaPlayer();
            backgroundMusicPlayer.Open(new Uri("C:\\Users\\Bruno\\RiderProjects\\SpaceInvaders\\SpaceInvaders\\Assets\\background_music.wav"));  // Caminho para o arquivo de música de fundo no formato WAV
            backgroundMusicPlayer.MediaEnded += BackgroundMusicPlayer_MediaEnded;  // Para quando a música terminar
            backgroundMusicPlayer.Volume = 0.6;
            backgroundMusicPlayer.Play();  // Começa a tocar a música
            
            gameOverMusicPlayer = new MediaPlayer();
            gameOverMusicPlayer.Open(new Uri("C:\\Users\\Bruno\\RiderProjects\\SpaceInvaders\\SpaceInvaders\\Assets\\gameover.wav"));  // Caminho para o arquivo de música de Game Over no formato WAV
            gameOverMusicPlayer.Volume = 0.6;  // Ajuste o volume conforme necessário

            
        }
        
        public void ShowExplosionGif(double x, double y)
        {
            Console.WriteLine("Exibindo GIF de explosão...");

            Image explosionImage = new Image
            {
                Width = 100,
                Height = 100
            };

            // Certifique-se de que o caminho do GIF está correto
            var gifSource = new BitmapImage(new Uri("pack://application:,,,/Assets/explosion.gif"));
            ImageBehavior.SetAnimatedSource(explosionImage, gifSource);

            Canvas.SetLeft(explosionImage, x);  // Posiciona o GIF horizontalmente
            Canvas.SetTop(explosionImage, y);   // Posiciona o GIF verticalmente

            // Verifique se o GameCanvas está acessível e visível
            if (GameCanvas != null)
            {
                Console.WriteLine("GameCanvas encontrado. Adicionando o GIF.");
                GameCanvas.Children.Add(explosionImage);  // Adiciona o GIF ao Canvas

                // Configura um temporizador para remover o GIF após 1.5 segundos
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1) // Tempo que o GIF ficará visível
                };

                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    Console.WriteLine("Removendo o GIF de explosão após 1.5 segundos.");
                    GameCanvas.Children.Remove(explosionImage);  // Remove a animação após o tempo
                };

                timer.Start();
            }
            else
            {
                Console.WriteLine("GameCanvas não está acessível ou não foi inicializado.");
            }
        }


        


        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameViewModel.State.IsGameOver)
            {
                gameViewModel.UpdateGame();
                RenderGame();
            }
            else if (!isScoreSaved)  // Verifica se o placar já foi salvo
            {
                isScoreSaved = true;  // Marca que o placar foi salvo
                ShowEndGameOptions();
                SaveScore();  // Salva a pontuação quando o jogo termina
            }
        }
        
        private void SaveScore()
        {
            // Cria a nova entrada de leaderboard com o nome do jogador e sua pontuação
            var leaderboardEntry = new LeaderboardEntry
            {
                Name = playerName,
                Score = gameViewModel.State.Score
            };

            // Salva a entrada no LeaderboardManager
            LeaderboardManager.SaveToLeaderboard(leaderboardEntry);

            // Atualiza o Leaderboard do GameViewModel com a lista salva
            gameViewModel.Leaderboard = new ObservableCollection<LeaderboardEntry>(LeaderboardManager.LeaderboardEntries);
        }


        public void ShowEndGameOptions()
        {
            // Cria um temporizador para esperar 4 segundos antes de exibir o painel de fim de jogo
            DispatcherTimer delayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            delayTimer.Tick += (sender, args) =>
            {
                delayTimer.Stop();  // Para o timer após disparar

                // Toca o som de Game Over e para a música de fundo
                gameOverMusicPlayer.Play();
                backgroundMusicPlayer.Stop();

                // Define o DataContext e exibe o painel com as opções de fim de jogo
                EndGameOptionsPanel.DataContext = gameViewModel;
                EndGameOptionsPanel.Visibility = Visibility.Visible;
            };
            delayTimer.Start();
        }






        public void RenderGame()
        {
            GameCanvas.Children.Clear();

            // Renderiza o jogador
            Image playerImage = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Assets/player.png")),
                Width = 40,
                Height = 40
            };
            Canvas.SetLeft(playerImage, gameViewModel.Player.X);
            Canvas.SetTop(playerImage, gameViewModel.Player.Y);
            GameCanvas.Children.Add(playerImage);

            // Renderiza os inimigos
            foreach (var enemy in gameViewModel.Enemies)
            {
                Image enemyImage = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Assets/inimigo.png")),
                    Width = 40,
                    Height = 40
                };
                Canvas.SetLeft(enemyImage, enemy.X);
                Canvas.SetTop(enemyImage, enemy.Y);
                GameCanvas.Children.Add(enemyImage);
            }
            
            foreach (var boss in gameViewModel.Bosses)
            {
                string bossImagePath;
    
                // Verifica se o boss atual é do tipo SupremeBoss
                if (boss is SupremeBoss)
                {
                    bossImagePath = "pack://application:,,,/Assets/supreme_boss.png";
                }
                else
                {
                    bossImagePath = "pack://application:,,,/Assets/inimigo1.png";
                }

                Image bossImage = new Image
                {
                    Source = new BitmapImage(new Uri(bossImagePath)),
                    Width = boss is SupremeBoss ? 120 : 80,  // Aumenta o tamanho do Boss Supremo
                    Height = boss is SupremeBoss ? 120 : 80
                };

                Canvas.SetLeft(bossImage, boss.X);
                Canvas.SetTop(bossImage, boss.Y);
                GameCanvas.Children.Add(bossImage);
            }



            // Renderiza os tiros
            foreach (var bullet in gameViewModel.Bullets)
            {
                Rectangle bulletRect = new Rectangle
                {
                    Width = 5,
                    Height = 15,
                    Fill = bullet.IsPlayerBullet ? Brushes.DeepSkyBlue : Brushes.Red  // Cor diferenciada para o jogador e inimigo
                };
                Canvas.SetLeft(bulletRect, bullet.X);
                Canvas.SetTop(bulletRect, bullet.Y);
                GameCanvas.Children.Add(bulletRect);
            }

            // Renderiza os escudos
            foreach (var shield in gameViewModel.Shields)
            {
                Brush shieldColor = GetShieldColor(shield.Health);
                Rectangle shieldRect = new Rectangle
                {
                    Width = 40,
                    Height = 20,
                    Fill = shieldColor
                };
                Canvas.SetLeft(shieldRect, shield.X);
                Canvas.SetTop(shieldRect, shield.Y);
                GameCanvas.Children.Add(shieldRect);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Left) gameViewModel.MovePlayerLeft();
            if (e.Key == System.Windows.Input.Key.Right) gameViewModel.MovePlayerRight();
            if (e.Key == System.Windows.Input.Key.Space) gameViewModel.PlayerShoot();
        }
        
        private void BackgroundMusicPlayer_MediaEnded(object sender, EventArgs e)
        {
            // Cheque se a música de fundo já foi parada, se sim, não reinicie
            if (!gameViewModel.State.IsGameOver)
            {
                backgroundMusicPlayer.Position = TimeSpan.Zero;  // Volta o tempo para o início
                backgroundMusicPlayer.Play();  // Reproduz a música novamente
            }
        }


        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Left) gameViewModel.StopMovingLeft();
            if (e.Key == System.Windows.Input.Key.Right) gameViewModel.StopMovingRight();
        }

        private void ContinuePlayingButton_Click(object sender, RoutedEventArgs e)
        {
            backgroundMusicPlayer.Play();
            gameOverMusicPlayer.Stop();  // Para a música de Game Over quando o jogo for reiniciado

            Console.WriteLine("Continuando o jogo...");  // Mensagem de log para depuração
            gameViewModel.RestartGame();  // Reinicia o jogo

            // Limpa a tela de "Game Over"
            EndGameOptionsPanel.Visibility = Visibility.Collapsed;  // Esconde as opções de fim de jogo
    
            // Reinicia o timer para garantir que o jogo continue
            gameTimer.Start();  // Inicia novamente o ciclo de atualização do jogo

            // Reinicia o ciclo de atualização imediatamente após o "Continuar"
            gameViewModel.UpdateGame();  // Chama UpdateGame para garantir que o jogo seja atualizado imediatamente
        }



        private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var mainMenuWindow = new MainMenu();  // Volta ao menu principal
            mainMenuWindow.Show();
            this.Close();  // Fecha a janela do jogo
            backgroundMusicPlayer.Stop();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();  // Encerra o jogo
        }
        
        
        private Brush GetShieldColor(int health)
        {
            // Dependendo da saúde da barricada, mudamos a cor
            if (health == 7)
                return Brushes.Purple; // Barricada intacta
            else if (health == 6)
                return Brushes.MediumPurple; // Barricada com um pouco de dano
            else if (health == 5)
                return Brushes.Violet; // Barricada com mais dano
            else if (health == 4)
                return Brushes.Plum; // Barricada com um pouco de dano
            else if (health == 3)
                return Brushes.PaleVioletRed; // Barricada com mais dano
            else if (health == 2)
                return Brushes.Orange; // Barricada com um pouco de dano
            else if (health == 1)
                return Brushes.Red; // Barricada com mais dano
            
            else
                return Brushes.Gray; // Barricada destruída
        }
    }
}
