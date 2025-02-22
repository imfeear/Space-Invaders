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



public class SupremeBoss : Boss
{
    private int specialAttackCooldown;
    private int teleportCooldown;
    private int meteorCooldown;
    private int initialHealth;
    private Random random = new Random();

    public SupremeBoss(double x, double y, int difficultyLevel) : base(x, y)
    {
        initialHealth = 50 + (difficultyLevel * 20);
        Health = initialHealth;  // Vida inicial do Boss Supremo
        BossDamage = 5 + difficultyLevel;
        BossShield = 20 + (difficultyLevel * 5);
        AttackMode = "Frenzy";
        Speed = 2.0 + (difficultyLevel * 0.3);

        // Cooldowns iniciais
        specialAttackCooldown = 300 - (difficultyLevel * 10);
        teleportCooldown = 500;
        meteorCooldown = 250 - (difficultyLevel * 10);

        // Evita valores negativos nos cooldowns
        specialAttackCooldown = Math.Max(130, specialAttackCooldown);
        teleportCooldown = Math.Max(320, teleportCooldown);
        meteorCooldown = Math.Max(100, meteorCooldown);
    }

    public void Attack(Player player)
    {
        player.Lives -= BossDamage;
    }

    public void Shoot(ObservableCollection<Bullet> bullets)
    {
        double healthPercentage = (double)Health / initialHealth;
        double fireRate = 0.05 + (1 - healthPercentage) * 0.05; // Aumenta quando a vida cai

        if (random.NextDouble() < fireRate)
        {
            var bullet = new Bullet(this.X + 30, this.Y + 60, 8 + Phase, false);
            bullets.Add(bullet);
        }
    }

    public void CircularAttack(ObservableCollection<Bullet> bullets)
    {
        double healthPercentage = (double)Health / initialHealth;

        // Ajusta a frequência conforme a vida diminui
        int attackFrequency = (int)(250 - (100 * (1 - healthPercentage)));

        if (specialAttackCooldown <= 0)
        {
            int bulletCount = 30 + (int)((1 - healthPercentage) * 30); // Aumenta conforme perde vida
            double angleStep = 360.0 / bulletCount;

            for (int i = 0; i < bulletCount; i++)
            {
                double angle = i * angleStep;
                double radian = Math.PI * angle / 180;
                double bulletSpeedX = Math.Cos(radian) * (5 + Phase);
                double bulletSpeedY = Math.Sin(radian) * (5 + Phase);

                var bullet = new Bullet(this.X + 15, this.Y + 60, 0, false)
                {
                    SpeedX = bulletSpeedX,
                    SpeedY = bulletSpeedY
                };
                bullets.Add(bullet);
            }

            specialAttackCooldown = Math.Max(80, attackFrequency); // Mantém um limite inferior
        }
    }

    public void MeteorShower(ObservableCollection<Bullet> bullets)
    {
        double healthPercentage = (double)Health / initialHealth;
        int meteorCount = 6 + (int)((1 - healthPercentage) * 6); // Mais meteoros com vida baixa

        if (meteorCooldown <= 0)
        {
            for (int i = 0; i < meteorCount; i++)
            {
                double xPos = random.Next(50, 750);
                var bullet = new Bullet(xPos, this.Y + 60, 10 + Phase, false);
                bullets.Add(bullet);
            }
            meteorCooldown = Math.Max(90, 200 - (int)((1 - healthPercentage) * 100));
        }
    }

    public void Teleport()
    {
        double healthPercentage = (double)Health / initialHealth;

        if (healthPercentage <= 0.8 && teleportCooldown <= 0) // Só começa a teleportar abaixo de 80% de vida
        {
            this.X = random.Next(100, 700);
            this.Y = random.Next(50, 300);
            Console.WriteLine($"⚡ Supreme Boss se teletransportou para {this.X}, {this.Y}!");

            // Ajuste da frequência de teleporte conforme a vida diminui
            if (healthPercentage > 0.5)
                teleportCooldown = 350; // Frequência moderada entre 80% e 50% de vida
            else if (healthPercentage > 0.2)
                teleportCooldown = 250; // Frequência maior entre 50% e 20% de vida
            else
                teleportCooldown = 150; // Frequência alta abaixo de 20% de vida
        }
    }

    public void UpdateBoss(ObservableCollection<Bullet> bullets)
    {
        // Atualiza cooldowns
        if (specialAttackCooldown > 0) specialAttackCooldown--;
        if (teleportCooldown > 0) teleportCooldown--;
        if (meteorCooldown > 0) meteorCooldown--;

        // Executa ataques conforme a fase e a vida
        CircularAttack(bullets);
        if (Phase >= 2) MeteorShower(bullets);

        // Executa teleporte com maior frequência conforme perde vida
        Teleport();

        // Aumenta a velocidade a cada fase
        Speed = 2.5 + (Phase * 0.4);
    }

    public void TakeDamage(int damage)
    {
        if (BossShield > 0)
        {
            int shieldDamage = Math.Min(damage, BossShield);
            BossShield -= shieldDamage;
            damage -= shieldDamage;
        }
        Health -= damage;
    }
}

}







