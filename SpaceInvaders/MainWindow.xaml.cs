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

namespace SpaceInvaders
{
    public partial class MainWindow : Window
    {
        
        private MediaPlayer backgroundMusicPlayer;
        private GameViewModel gameViewModel;
        private string playerName;
        private DispatcherTimer gameTimer;
        private bool isScoreSaved = false;
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
            backgroundMusicPlayer.Open(new Uri("pack://application:,,,/Assets/background_music.wav"));  // Caminho para o arquivo de música de fundo no formato WAV
            backgroundMusicPlayer.MediaEnded += BackgroundMusicPlayer_MediaEnded;  // Para quando a música terminar
            backgroundMusicPlayer.Play();  // Começa a tocar a música
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
            // Define o DataContext para o painel de fim de jogo
            EndGameOptionsPanel.DataContext = gameViewModel;  // Usando o DataContext do jogo
            EndGameOptionsPanel.Visibility = Visibility.Visible;  // Exibe o painel com as opções
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
            backgroundMusicPlayer.Position = TimeSpan.Zero;  // Volta o tempo para o início
            backgroundMusicPlayer.Play();  // Reproduz a música novamente
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Left) gameViewModel.StopMovingLeft();
            if (e.Key == System.Windows.Input.Key.Right) gameViewModel.StopMovingRight();
        }

        private void ContinuePlayingButton_Click(object sender, RoutedEventArgs e)
        {
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
