using SpaceInvaders.Models;  // Agora que LeaderboardEntry foi movida para o namespace correto

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;  // Usando o System.Text.Json para serialização e desserialização

public static class LeaderboardManager
{
    private static string leaderboardFile = "leaderboard.json";
    
    // Lista de entradas do placar
    public static List<LeaderboardEntry> LeaderboardEntries { get; set; } = new List<LeaderboardEntry>();

    static LeaderboardManager()
    {
        LoadLeaderboard();
    }

    // Salva no arquivo JSON
    public static void SaveToLeaderboard(LeaderboardEntry entry)
    {
        LeaderboardEntries.Add(entry);
        LeaderboardEntries = LeaderboardEntries.OrderByDescending(e => e.Score).Take(10).ToList(); // Mantém apenas os top 10
        try
        {
            string json = JsonSerializer.Serialize(LeaderboardEntries);
            File.WriteAllText(leaderboardFile, json); // Usando o JsonSerializer para salvar no arquivo
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving leaderboard: {ex.Message}");
        }
    }


    // Carrega o placar de um arquivo
    public static void LoadLeaderboard()
    {
        if (File.Exists(leaderboardFile))
        {
            try
            {
                var json = File.ReadAllText(leaderboardFile);
                if (string.IsNullOrWhiteSpace(json))
                {
                    // Caso o arquivo esteja vazio, inicializa a lista vazia
                    LeaderboardEntries = new List<LeaderboardEntry>();
                }
                else
                {
                    LeaderboardEntries = JsonSerializer.Deserialize<List<LeaderboardEntry>>(json) ?? new List<LeaderboardEntry>();
                }
            }
            catch (JsonException ex)
            {
                // Caso o arquivo não seja um JSON válido, você pode registrar um erro ou criar um arquivo vazio
                Console.WriteLine($"Erro ao ler o arquivo JSON: {ex.Message}");
                LeaderboardEntries = new List<LeaderboardEntry>(); // Garante que não ocorra falha
            }
        }
        else
        {
            // Se o arquivo não existir, inicializa uma lista vazia
            LeaderboardEntries = new List<LeaderboardEntry>();
        }
    }


}