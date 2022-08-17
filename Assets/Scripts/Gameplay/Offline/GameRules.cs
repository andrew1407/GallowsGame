using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Difficulty = DifficultyStats.Difficulty;

public class GameRules
{
    private readonly string[] _words;

    public GameRules(string[] words) => _words = words;

    public Difficulty ParseDifficulty(string data, out bool multipleLetters)
    {
        bool matches = Regex.IsMatch(data, @"^[1-4]{1}(\+\+)?$");
        if (!matches) throw new InvalidDataException();
        multipleLetters = !data.EndsWith("++");
        int difficulty = int.Parse(data[0].ToString());
        return (Difficulty)difficulty;
    }

    public GameProgress GetInitialPlayerData(Difficulty difficulty) => new() {
        Stage = difficulty == Difficulty.NO_CHALLANGE ? GameStageLabels.Prologue : GameStageLabels.Gameplay,
        Difficulty = DifficultyStats.StringifyDifficulty(difficulty),
        Tries = DifficultyStats.Tries(difficulty),
        Guessed = Array.Empty<string>(),
    };

    public string[] PickWord(out string[] template)
    {
        var random = new Random();
        int index = random.Next(_words.Length);
        string word = _words[index];
        string[] chars = word.Select(ch => ch.ToString()).ToArray();
        template = chars.Select(_ => (string)null).ToArray();
        return chars;
    }

    public int[] FindGuessed(string data, string[] lettersLeft, bool multipleLetters = false)
    {
        string comparable = data?.ToLower();
        List<int> foundGuessed = new();
        if (!string.IsNullOrEmpty(comparable))
            for (int i = 0; i < lettersLeft.Length; ++i)
            {
                string letter = lettersLeft[i];
                if (letter?.ToLower() != comparable) continue;
                foundGuessed.Add(i);
                if (!multipleLetters) break;
            }
        return foundGuessed.ToArray();
    }
}
