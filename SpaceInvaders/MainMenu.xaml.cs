using System.Windows;
using SpaceInvaders.ViewModels;

namespace SpaceInvaders
{
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
            GameViewModel gameViewModel = new GameViewModel();  // Inicializa o GameViewModel
            DataContext = gameViewModel;  // Configura o DataContext com o GameViewModel
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Cria a instância da janela PlayerNameDialog
            var playerNameDialog = new PlayerNameDialog();

            // Exibe a janela e aguarda até que o jogador insira o nome
            playerNameDialog.ShowDialog();

            // O nome do jogador agora estará disponível em playerNameDialog.PlayerName
            string playerName = playerNameDialog.PlayerName;

            // Verifica se o nome foi fornecido antes de abrir a janela do jogo
            if (!string.IsNullOrEmpty(playerName))
            {
                // Passa o nome para o MainWindow e inicializa o GameViewModel
                var gameWindow = new MainWindow(playerName);  
                gameWindow.Show();
                this.Close();  // Fecha a janela do menu
            }
            else
            {
                // Se não houver nome, não faça nada ou exiba uma mensagem de erro
                MessageBox.Show("Por favor, insira um nome válido.");
            }
        }

        

        private void ScoreboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Obtém o GameViewModel do DataContext da janela principal
            GameViewModel gameViewModel = (GameViewModel)this.DataContext;
    
            // Cria uma instância da janela de placar e passa o GameViewModel
            LeaderboardWindow leaderboardWindow = new LeaderboardWindow(gameViewModel);

            // Exibe a janela
            leaderboardWindow.Show();
        }


        
    }
}