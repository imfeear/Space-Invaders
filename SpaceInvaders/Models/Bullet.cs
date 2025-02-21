namespace SpaceInvaders.Models;

public class Bullet
{
    public double X { get; set; }
    public double Y { get; set; }
    public double SpeedX { get; set; }
    public double SpeedY { get; set; }
    public bool IsPlayerBullet { get; set; }

    public Bullet(double x, double y, double speed, bool isPlayerBullet)
    {
        X = x;
        Y = y;
        SpeedX = 0;  // Não há movimento horizontal para o tiro do jogador
        SpeedY = speed;  // Inicializando SpeedY com a mesma velocidade
        IsPlayerBullet = isPlayerBullet;
    }

    public void Move()
    {
        X += SpeedX;  // Move o tiro horizontalmente
        Y += SpeedY;  // Move o tiro verticalmente
    }
}
