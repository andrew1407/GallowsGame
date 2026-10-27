using System;
using System.IO;
using System.Linq;
using UnityEngine;

using Difficulty = DifficultyStats.Difficulty;

// Text data ships as Resources (not StreamingAssets) so it loads synchronously on every platform, WebGL included
public class ResourceLoader
{
    private const string _wordsFile = "words";

    private const string _messageFile = "message";

    private const string _difficultiesFile = "difficulty";

    private static readonly string[] _lineSeparators = { "\r\n", "\n" };

    public string LoadDifficlutiesDescription()
    {
        string loaded = load(_difficultiesFile);
        var difficulties = Enum.GetNames(typeof(Difficulty));
        return difficulties.Aggregate(loaded, setDifficultyTries);
    }

    public string LoadLossMessage() => load(_messageFile);

    public string[] LoadWords() => load(_wordsFile)
        .Split(_lineSeparators, StringSplitOptions.RemoveEmptyEntries)
        .Select(word => word.Trim())
        .Where(word => word.Length > 0)
        .ToArray();

    private string load(string file)
    {
        var asset = Resources.Load<TextAsset>(file);
        if (asset == null) throw new FileNotFoundException($"Resource \"{file}\" is missing");
        return asset.text;
    }

    private string setDifficultyTries(string data, string difficulty)
    {
        bool noChallangeMode = DifficultyStats.Is(difficulty, Difficulty.NO_CHALLANGE);
        string value = noChallangeMode ? "overmnogo" : DifficultyStats.Tries(difficulty).ToString();
        return data.Replace(difficulty, value);
    }
}
