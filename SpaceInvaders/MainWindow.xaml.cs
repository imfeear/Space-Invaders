using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpaceInvaders.ViewModels;

namespace SpaceInvaders
{
    public partial class MainWindow : Window
    {
        private GameViewModel gameViewModel;
        
        public MainWindow()
        {
            InitializeComponent();
            gameViewModel = new GameViewModel();
            DataContext = gameViewModel;
            CompositionTarget.Rendering += GameLoop;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameViewModel.State.IsGameOver)
            {
                gameViewModel.UpdateGame();
                RenderGame();
            }
        }

        public void RenderGame()
        {
            GameCanvas.Children.Clear();

            // Renderiza o jogador
            Image playerImage = new Image
            {
                Source = new BitmapImage(new Uri("https://cdn-icons-png.flaticon.com/512/706/706026.png")), // URL do jogador
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
                    Source = new BitmapImage(new Uri("https://png.pngtree.com/png-vector/20220623/ourmid/pngtree-space-invaders-character-game-play-png-image_5289513.png")),
                    Width = 40,
                    Height = 40
                };
                Canvas.SetLeft(enemyImage, enemy.X);
                Canvas.SetTop(enemyImage, enemy.Y);
                GameCanvas.Children.Add(enemyImage);
            }

            // Renderiza os tiros
            foreach (var bullet in gameViewModel.Bullets)
            {
                Rectangle bulletRect = new Rectangle
                {
                    Width = 5,
                    Height = 15,
                    Fill = Brushes.LightSeaGreen
                };
                Canvas.SetLeft(bulletRect, bullet.X);
                Canvas.SetTop(bulletRect, bullet.Y);
                GameCanvas.Children.Add(bulletRect);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Left) gameViewModel.MovePlayerLeft();
            if (e.Key == System.Windows.Input.Key.Right) gameViewModel.MovePlayerRight();
            if (e.Key == System.Windows.Input.Key.Space) gameViewModel.PlayerShoot();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Left) gameViewModel.StopMovingLeft();
            if (e.Key == System.Windows.Input.Key.Right) gameViewModel.StopMovingRight();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            gameViewModel.RestartGame();
            MessageBox.Visibility = Visibility.Hidden;
            RestartButton.Visibility = Visibility.Hidden;
        }
    }
}
