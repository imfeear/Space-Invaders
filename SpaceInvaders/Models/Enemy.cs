using System.Collections.ObjectModel;

namespace SpaceInvaders.Models
{
    public class Enemy
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; } = 2; // Velocidade padrão do inimigo
        public int Health { get; set; } = 1; // Saúde padrão do inimigo

        public Enemy(double x, double y)
        {
            X = x;
            Y = y;
        }

        // Método para diminuir a saúde do inimigo
        public void TakeDamage()
        {
            Health--;
        }
    }

public class Boss : Enemy
{
    public double BossSpeed { get; set; }
    public int BossDamage { get; set; }
    public int BossShield { get; set; }
    public string AttackMode { get; set; } // Pode ser "Normal", "Frenzy", "Circular"
    public string BossBulletType { get; set; } // Tipo de tiro: "Normal", "Triple", etc.
    public int Phase { get; set; }  // Fase do boss, por exemplo, 1 ou 2 (indica o estágio da batalha)

    // Definindo uma variável para o ataque especial
    private bool isSpecialAttackReady = false;
    private int specialAttackCooldown = 200;  // O ataque especial pode ser usado após 200 ciclos

    public Boss(double x, double y) : base(x, y)
    {
        Health = 10;  // Mais saúde que um inimigo comum
        Speed = 1;    // O Boss será mais lento, mas mais resistente
        BossSpeed = 4;  // O Boss pode ter uma velocidade um pouco maior ou menor
        BossDamage = 2;  // O Boss causa mais dano
        BossShield = 5;  // O Boss tem um escudo que absorve até 5 de dano
        AttackMode = "Normal";  // Começa com um ataque normal
        BossBulletType = "Normal";  // Tipo de tiro inicial
        Phase = 1;  // Começa na fase 1
    }
    
    public void Attack(Player player)
    {
        if (AttackMode == "Frenzy")
        {
            player.Lives -= BossDamage * 2;  // Se estiver em modo Frenzy, o dano é duplicado
        }
        else
        {
            player.Lives -= BossDamage;  // No modo normal, o dano é normal
        }
    }

    // Método unificado para atacar e atirar
    // Método para disparar tiros do Boss
    public void Shoot(ObservableCollection<Bullet> bullets)
    {
        if (BossBulletType == "Normal")
        {
            // Chance de disparo normal
            if (new Random().NextDouble() < 0.02)  
            {
                var bullet = new Bullet(this.X + 15, this.Y + 60, 5, false);  // Tiro indo para baixo
                bullets.Add(bullet);
            }
        }
        else if (BossBulletType == "Triple")
        {
            // Chance de disparar três tiros
            if (new Random().NextDouble() < 0.03)  
            {
                var bullet1 = new Bullet(this.X + 5, this.Y + 60, 5, false);
                var bullet2 = new Bullet(this.X + 15, this.Y + 60, 5, false);
                var bullet3 = new Bullet(this.X + 25, this.Y + 60, 5, false);

                bullets.Add(bullet1);
                bullets.Add(bullet2);
                bullets.Add(bullet3);
            }
        }
    }


    // Método para o Boss atirar de forma circular (ataque especial)
    public void CircularAttack(ObservableCollection<Bullet> bullets)
    {
        if (isSpecialAttackReady && AttackMode == "Circular") // Ativa o ataque circular somente na fase 3
        {
            int bulletCount = 40;  // Número de tiros no círculo (pode aumentar conforme necessário)
        
            // Criamos uma distribuição mais densa para que os tiros saiam em 360 graus
            double angleStep = 360.0 / bulletCount;  // Passo do ângulo para os tiros

            // Dispara tiros em todas as direções
            for (int i = 0; i < bulletCount; i++)
            {
                // Calcula o ângulo em radianos para cada tiro
                double angle = i * angleStep;  // Cada tiro é distribuído ao longo de 360 graus
                double radian = Math.PI * angle / 180;

                // Calcula a velocidade dos tiros em ambos os eixos, horizontal e vertical
                // Multiplica a velocidade para ajustar o alcance e direção
                double bulletSpeedX = Math.Cos(radian) * 5; // Movimento horizontal
                double bulletSpeedY = Math.Sin(radian) * 5; // Movimento vertical

                // Cria o tiro e define a posição inicial (a posição Y é ligeiramente ajustada para evitar sobreposição)
                var bullet = new Bullet(this.X + 15, this.Y + 60, 0, false);  // Cria o tiro do boss

                // Atribui a velocidade calculada para o tiro
                bullet.SpeedX = bulletSpeedX;
                bullet.SpeedY = bulletSpeedY;

                // Adiciona o tiro à lista de tiros
                bullets.Add(bullet);
            }

            // Após o ataque especial, desabilita até o cooldown
            isSpecialAttackReady = false;
        }
    }


    
    

    // Método para reduzir o escudo do Boss
    public void TakeDamage(int damage)
    {
        if (BossShield > 0)
        {
            int shieldDamage = Math.Min(damage, BossShield);
            BossShield -= shieldDamage;  // Reduz o escudo primeiro
            damage -= shieldDamage;  // O restante vai para a saúde
        }

        Health -= damage;
    }

    // Verifica se o boss passou para a próxima fase
    public void CheckPhase()
    {
        if (Health <= 5 && Phase == 1)
        {
            Phase = 2;  // Muda para a fase 2 quando a saúde chegar a 5
            AttackMode = "Frenzy";  // Muda o modo de ataque para Frenzy
            BossBulletType = "Triple";  // O boss começa a disparar 3 tiros
        }
        else if (Health <= 2 && Phase == 2)
        {
            Phase = 3;  // Muda para a fase 3 quando a saúde chegar a 2
            AttackMode = "Circular";  // Muda o modo de ataque para Circular
            BossBulletType = "Triple";  // O boss ainda pode continuar a disparar 3 tiros, se necessário
        }
        // A fase 3 já tem um ataque circular ativado
    }


    // Reseta o cooldown para o ataque especial a cada fase
    public void ResetSpecialAttackCooldown()
    {
        specialAttackCooldown = 200;  // Pode ser ajustado conforme o comportamento desejado
        isSpecialAttackReady = true;  // Permite o uso do ataque especial após o cooldown
    }
}



}