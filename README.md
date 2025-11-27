# Space Invaders - Jogo em C# com WPF

![Space Invaders](\SpaceInvaders\img\screen-example.png)

**Space Invaders** é uma implementação clássica do jogo de arcade "Space Invaders", criado usando a linguagem de programação C# e a framework WPF (Windows Presentation Foundation). O jogo oferece uma experiência divertida de destruição de inimigos, com vários desafios como a ativação de bônus e a batalha contra um Boss Supremo.

## Funcionalidades

- **Jogador e Movimentos**: O jogador controla uma nave que pode se mover à esquerda e à direita, atirando em inimigos que descem pela tela.
- **Inimigos e Boss Supremo**: Uma série de inimigos avançam na tela, e a cada 50.000 pontos o Boss Supremo aparece.
- **Bônus de Tiro Duplo**: Ao alcançar pontos específicos, o jogador pode ativar um bônus de tiro duplo.
- **Colisões**: O jogo verifica colisões entre tiros, inimigos e o jogador, o que resulta na perda de vidas ou destruição dos inimigos.
- **Barricadas (Escudos)**: O jogador pode se proteger atrás de escudos que podem ser destruídos pelos tiros dos inimigos.
- **Leaderboard**: O jogo mantém um leaderboard onde o nome do jogador e sua pontuação são registrados.

## Requisitos

Para rodar o projeto, você precisará do seguinte:

- **.NET Framework 4.7.2 ou superior**: O jogo foi desenvolvido usando C# e WPF, então é necessário ter o .NET Framework instalado em seu sistema.
- **Visual Studio 2019 ou superior**: Para editar e executar o código, recomendamos usar o Visual Studio com suporte para C# e WPF.

## Estrutura do Projeto

O projeto é estruturado da seguinte forma:

/SpaceInvaders
│
├── Assets/                      # Contém arquivos de mídia (sons e imagens)
│   ├── background_music.wav      # Música de fundo do jogo
│   ├── shot_sound.wav            # Som de disparo
│   ├── inimigo.png               # Imagem de um inimigo
│   ├── inimigo1.png              # Imagem alternativa de inimigo
│   ├── player.png                # Imagem do jogador
│   └── supreme_boss.png          # Imagem do Boss Supremo
│
├── Models/                       # Contém as classes de lógica de jogo
│   ├── Bullet.cs                 # Classe que representa os tiros
│   ├── Enemy.cs                  # Classe que representa os inimigos
│   ├── Player.cs                 # Classe que representa o jogador
│   ├── Boss.cs                   # Classe base para os bosses
│   ├── SupremeBoss.cs            # Classe para o Boss Supremo
│   ├── Shield.cs                 # Classe para os escudos
│   └── LeaderboardEntry.cs       # Representa uma entrada no leaderboard
│
├── ViewModels/                   # Contém a lógica de controle de jogo (ViewModels)
│   ├── GameViewModel.cs          # ViewModel que controla o estado do jogo
│   └── LeaderboardManager.cs     # Gerencia a persistência do leaderboard
│
├── Views/                         # Contém as views (arquivos XAML e código por trás)
│   ├── MainWindow.xaml           # Interface principal do jogo
│   ├── MainWindow.xaml.cs        # Código por trás da interface principal
│   ├── MainMenu.xaml             # Tela de menu principal
│   ├── MainMenu.xaml.cs          # Código por trás do menu principal
│   └── EndGameOptions.xaml       # Tela de fim de jogo
│
├── App.xaml                      # Configurações gerais da aplicação
├── App.xaml.cs                   # Código por trás da configuração do aplicativo
└── SpaceInvaders.sln             # Arquivo da solução (Visual Studio)



## Como Usar

### 1. **Clonando o Repositório**

Para começar, clone este repositório para o seu computador.

```bash
git clone https://github.com/seu-usuario/space-invaders.git
```

## Controles

- **Movimento:** Use as teclas seta para a esquerda e seta para a direita para mover a nave.
- **Movimento:**Atirar: Pressione a tecla Espaço para disparar.
- **Movimento:**Pausar/Continuar: Selecione as opções na tela de fim de jogo.


# Como funciona
## Game Loop
O jogo é baseado em um loop de atualização de 60 FPS (Frames por segundo). A cada ciclo, ele verifica o estado do jogo (se o jogador perdeu todas as vidas ou se o jogo foi ganho), atualiza as posições dos elementos (jogador, inimigos, tiros) e faz a renderização dos novos quadros na tela.

## Sons
O jogo utiliza arquivos de som no formato .wav para os efeitos de som:

## Música de Fundo 
- (background_music.wav): Toca continuamente enquanto o jogo está em execução.
- Som de Disparo (shot_sound.wav): Toca sempre que o jogador atira.

## Modificação de Sons
Se você deseja alterar os sons do jogo, basta substituir os arquivos .wav na pasta Assets com os novos sons desejados. Os arquivos devem ter o mesmo nome para garantir que o código funcione corretamente.