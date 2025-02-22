namespace SpaceInvaders.Models;

public class Shield
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Health { get; set; } = 7;

    public Shield(double x, double y)
    {
        X = x;
        Y = y;
    }

    public void TakeDamage()
    {
        Health--;
    }
}