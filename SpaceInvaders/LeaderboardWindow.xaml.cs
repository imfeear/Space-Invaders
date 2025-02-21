using System.Windows;
using SpaceInvaders.ViewModels;

namespace SpaceInvaders
{
    public partial class LeaderboardWindow : Window
    {
        public LeaderboardWindow(GameViewModel gameViewModel)
        {
            if (gameViewModel == null)
            {
                throw new ArgumentNullException(nameof(gameViewModel), "GameViewModel cannot be null.");
            }

            InitializeComponent();
            DataContext = gameViewModel;  // Define o DataContext com o GameViewModel
        }

    }
}