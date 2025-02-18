namespace SpaceInvaders.Models;

public class Enemy
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; } = 6;
    public bool CanShoot { get; set; }

    public Enemy(double x, double y, bool canShoot = false)
    {
        X = x;
        Y = y;
        CanShoot = canShoot;
    }

    public void Move(double speed)
    {
        X += speed;
    }

    public Bullet Shoot()
    {
        return new Bullet(X + 15, Y + 30, 10);
    }
}