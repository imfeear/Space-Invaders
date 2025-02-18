namespace SpaceInvaders.Models;

public class Player
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Lives { get; set; } = 3;
    public int Score { get; set; } = 0;
    public double Speed { get; set; } = 10;

    public Player(double x, double y)
    {
        X = x;
        Y = y;
    }

    public void MoveLeft()
    {
        X -= Speed;
    }

    public void MoveRight()
    {
        X += Speed;
    }

    public Bullet Shoot()
    {
        return new Bullet(X + 20, Y - 10, -15);
    }
}