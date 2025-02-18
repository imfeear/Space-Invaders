using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using SpaceInvaders.ViewModels;

namespace SpaceInvaders;

    public partial class MainWindow : Window
    {
        private GameViewModel _gameViewModel;
        private DispatcherTimer _gameTimer;

        public MainWindow()
        {
            InitializeComponent();
            _gameViewModel = new GameViewModel();
            DataContext = _gameViewModel;


            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            _gameViewModel.UpdateGame(); // Atualiza o estado do jogo
            UpdateUI(); // Atualiza a interface gráfica
        }

        private void UpdateUI()
        {
            // Remove apenas os elementos dinâmicos do jogo (player, inimigos, projéteis)
            var dynamicElements = GameCanvas.Children.OfType<UIElement>()
                .Where(e => !(e is TextBlock)) // Não remove os TextBlock
                .ToList();

            foreach (var element in dynamicElements)
            {
                GameCanvas.Children.Remove(element);
            }

            // Redesenha os elementos dinâmicos
            // Adiciona o jogador
            Rectangle player = new Rectangle
            {
                Width = 40,
                Height = 20,
                Fill = System.Windows.Media.Brushes.Lime
            };
            Canvas.SetTop(player, _gameViewModel.Player.Y);
            Canvas.SetLeft(player, _gameViewModel.Player.X);
            GameCanvas.Children.Add(player);

            // Adiciona os inimigos
            foreach (var enemy in _gameViewModel.Enemies)
            {
                Rectangle enemyRect = new Rectangle
                {
                    Width = 40,
                    Height = 40,
                    Fill = System.Windows.Media.Brushes.Red
                };
                Canvas.SetTop(enemyRect, enemy.Y);
                Canvas.SetLeft(enemyRect, enemy.X);
                GameCanvas.Children.Add(enemyRect);
            }

            // Adiciona os projéteis
            foreach (var bullet in _gameViewModel.Bullets)
            {
                Rectangle bulletRect = new Rectangle
                {
                    Width = 5,
                    Height = 15,
                    Fill = System.Windows.Media.Brushes.White
                };
                Canvas.SetTop(bulletRect, bullet.Y);
                Canvas.SetLeft(bulletRect, bullet.X);
                GameCanvas.Children.Add(bulletRect);
            }

            // Exibe mensagem de fim de jogo, se necessário
            if (_gameViewModel.State.IsGameOver)
            {
                _gameTimer.Stop();
                MessageBox.Visibility = Visibility.Visible;
                MessageBox.Text = _gameViewModel.Enemies.Count == 0 ? "VOCÊ VENCEU!" : "GAME OVER!";
                RestartButton.Visibility = Visibility.Visible; // Exibe o botão de reinício
            }
        }
        
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            // Reinicia o jogo criando uma nova instância de GameView
            MainWindow newGame = new MainWindow();
            newGame.Show();
            this.Close(); // Fecha a janela atual
        }



        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                _gameViewModel.MovePlayerLeft();
            if (e.Key == Key.Right)
                _gameViewModel.MovePlayerRight();
            if (e.Key == Key.Space)
                _gameViewModel.PlayerShoot();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                _gameViewModel.StopMovingLeft();
            if (e.Key == Key.Right)
                _gameViewModel.StopMovingRight();
        }
    }