using System;
using System.Linq;
using NUnit.Framework;

using Difficulty = DifficultyStats.Difficulty;

public class ResourceLoaderTests
{
    [Test]
    public void Should_Load_Non_Empty_Words()
    {
        ResourceLoader resourceLoader = new();

        string[] words = resourceLoader.LoadWords();

        Assert.IsNotEmpty(words);
        foreach (var word in words)
        {
            Assert.False(string.IsNullOrWhiteSpace(word), message: "Words list cannot contain blank lines");
            Assert.AreEqual(word.Trim(), word, message: $"Word \"{word}\" should be trimmed");
        }
    }

    [Test]
    public void Should_Load_Loss_Message()
    {
        ResourceLoader resourceLoader = new();

        string message = resourceLoader.LoadLossMessage();

        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    [Test]
    public void Should_Replace_Difficulty_Names_With_Tries_In_Description()
    {
        ResourceLoader resourceLoader = new();

        string description = resourceLoader.LoadDifficlutiesDescription();

        foreach (var difficulty in Enum.GetNames(typeof(Difficulty)))
            Assert.False(description.Contains(difficulty), message: $"Difficulty name {difficulty} should be replaced");

        Difficulty[] withTries = { Difficulty.DIFFICULT, Difficulty.NORMAL, Difficulty.EASY };
        foreach (var difficulty in withTries)
        {
            string tries = DifficultyStats.Tries(difficulty).ToString();
            Assert.True(description.Contains(tries), message: $"Description should contain {tries} tries of {difficulty}");
        }
        Assert.True(description.Contains("overmnogo"), message: $"{Difficulty.NO_CHALLANGE} tries should be replaced with a word");
    }

    [Test]
    public void Should_Give_Playable_Words_For_Game_Rules()
    {
        ResourceLoader resourceLoader = new();
        GameRules gameRules = new(resourceLoader.LoadWords());

        string[] letters = gameRules.PickWord(out string[] template);

        Assert.IsNotEmpty(letters);
        Assert.AreEqual(letters.Length, template.Length);
        Assert.True(letters.All(letter => letter.Length == 1), message: "Picked word should be split into letters");
    }
}
