using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using Difficulty = DifficultyStats.Difficulty;

public class ResourceLoader
{
    private const string _wordsFile = "words";

    private const string _messageFile = "message";

    private const string _difficultiesFile = "difficulty";

    public async Task<string> LoadDifficlutiesDescription()
    {
        string loaded = await File.ReadAllTextAsync(asPath(_difficultiesFile));
        var difficulties = Enum.GetNames(typeof(Difficulty));
        return difficulties.Aggregate(loaded, setDifficultyTries);
    }

    public Task<string> LoadLossMessage() => File.ReadAllTextAsync(asPath(_messageFile));

    public Task<string[]> LoadWords() => File.ReadAllLinesAsync(asPath(_wordsFile));

    private string asPath(string file) => $"{Application.streamingAssetsPath}/{file}.txt";

    private string setDifficultyTries(string data, string difficulty)
    {
        bool noChallangeMode = DifficultyStats.Is(difficulty, Difficulty.NO_CHALLANGE);
        string value = noChallangeMode ? "overmnogo" : DifficultyStats.Tries(difficulty).ToString();
        return data.Replace(difficulty, value);
    }
}
