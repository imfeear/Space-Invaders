using System;
using System.Windows;

namespace SpaceInvaders
{
    public partial class PlayerNameDialog : Window
    {
        public string PlayerName { get; set; } // Propriedade para armazenar o nome do jogador

        public PlayerNameDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerName = PlayerNameTextBox.Text; // Obtém o nome inserido pelo jogador
            if (!string.IsNullOrEmpty(PlayerName))  // Verifica se o nome não está vazio
            {
                this.DialogResult = true; // Define como true, indicando que o nome foi inserido
                this.Close();  // Fecha a janela de diálogo
            }
            else
            {
                MessageBox.Show("Please enter a valid name.");
            }
        }
    }
}