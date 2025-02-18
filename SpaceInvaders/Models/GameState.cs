using System.ComponentModel;

namespace SpaceInvaders.Models;

public class GameState : INotifyPropertyChanged
{
    private int _score;
    private int _lives;
    private bool _isGameOver;

    public int Score
    {
        get => _score;
        set
        {
            _score = value;
            OnPropertyChanged(nameof(Score));
        }
    }

    public int Lives
    {
        get => _lives;
        set
        {
            _lives = value;
            OnPropertyChanged(nameof(Lives));
        }
    }

    public bool IsGameOver
    {
        get => _isGameOver;
        set
        {
            _isGameOver = value;
            OnPropertyChanged(nameof(IsGameOver));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}