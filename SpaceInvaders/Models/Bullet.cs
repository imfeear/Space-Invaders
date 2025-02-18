namespace SpaceInvaders.Models;

public class Bullet
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; } // Velocidade do projétil

    public Bullet(double x, double y, double speed)
    {
        X = x;
        Y = y;
        Speed = speed; // Defina a velocidade ao criar o projétil
    }

    public void Move()
    {
        Y += Speed; // Atualiza a posição vertical do projétil
    }
}