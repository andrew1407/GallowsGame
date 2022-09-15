using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NUnit.Framework;

using Difficulty = DifficultyStats.Difficulty;

public class GameRulesTests
{
    [Test]
    public void Should_Parse_Difficulties_And_Set_No_Multiple_Letters_As_Default()
    {
        GameRules gameRules = new(Array.Empty<string>());
        Dictionary<int, Difficulty> testData = new() {
            {1, Difficulty.DIFFICULT},
            {2, Difficulty.NORMAL},
            {3, Difficulty.EASY},
            {4, Difficulty.NO_CHALLANGE},
        };

        foreach (var difficulty in testData)
        {
            Difficulty expected = difficulty.Value;
            string data = difficulty.Key.ToString();
            Difficulty actual = gameRules.ParseDifficulty(data, out bool multipleLetters);
            Assert.True(multipleLetters, message: $"Multiple letters should be true for {expected}");
            Assert.AreEqual(expected, actual, message: $"{expected} should equal {actual} for {difficulty.Key}");
        }
    }

    [Test]
    public void Should_Parse_Difficulties_With_Truthy_Multiple_Letters()
    {
        GameRules gameRules = new(Array.Empty<string>());
        string prefix = "++";
        Dictionary<int, Difficulty> testData = new() {
            {1, Difficulty.DIFFICULT},
            {2, Difficulty.NORMAL},
            {3, Difficulty.EASY},
            {4, Difficulty.NO_CHALLANGE},
        };

        foreach (var difficulty in testData)
        {
            Difficulty expected = difficulty.Value;
            string data = difficulty.Key.ToString() + prefix;
            Difficulty actual = gameRules.ParseDifficulty(data, out bool multipleLetters);
            Assert.False(multipleLetters, message: $"Multiple letters should be false for {expected}");
            Assert.AreEqual(expected, actual, message: $"{expected} should equal {actual} for {difficulty.Key}");
        }
    }

    [Test]
    public void Should_Throw_Invalid_Parsing_Exeption()
    {
        GameRules gameRules = new(Array.Empty<string>());
        string data = "sasik";

        Assert.Throws<InvalidDataException>(() => gameRules.ParseDifficulty(data, out bool multipleLetters));
    }

    [Test]
    public void Should_Pick_Word_From_Given_Word()
    {
        string[] words = { "sasik" };
        GameRules gameRules = new(words);

        string[] expected = words[0].Select(ch => ch.ToString()).ToArray();
        string[] actual = gameRules.PickWord(out string[] template);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Pick_Word_From_Given_Words_Array()
    {
        string[] words = { "sasik", "bobo" };
        GameRules gameRules = new(words);

        string[] picked = gameRules.PickWord(out string[] template);
        string actual = String.Join(string.Empty, picked);

        Assert.True(words.Contains(actual), message: $"Word \"{actual}\" isn't among given words");
    }

    [Test]
    public void Should_Give_Word_Template_With_Same_Size_And_Filled_With_Nulls()
    {
        string[] words = { "sasik" };
        GameRules gameRules = new(words);

        string[] picked = gameRules.PickWord(out string[] template);
        bool allNulls = template.All(str => str == null);
        int expected = picked.Length; 
        int actual = template.Length; 

        Assert.True(allNulls, message: $"Word guessed template should be fully filled with nulls");
        Assert.AreEqual(expected, actual, message: $"Word size ({expected}) should be equal template size ({actual})");
    }

    [Test]
    public void Should_Give_Empty_Array_Of_Found_Positions()
    {
        string[] words = { "sasik" };
        GameRules gameRules = new(words);
        string data = "b";

        string[] letters = words[0].Select(ch => ch.ToString()).ToArray();
        int[] actual = gameRules.FindGuessed(data, letters);

        Assert.IsEmpty(actual);
    }

    [Test]
    public void Should_Give_Found_Indices_Any_Case_With_Multiple_Letters()
    {
        string[] words = { "Sasik" };
        GameRules gameRules = new(words);
        string data = "s";
        bool multipleLetters = true;
        int[] expected = { 0, 2 };

        string[] letters = words[0].Select(ch => ch.ToString()).ToArray();
        int[] actual = gameRules.FindGuessed(data, letters, multipleLetters);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Give_Found_Indices_Any_Case_Without_Multiple_Letters()
    {
        string[] words = { "Sasik" };
        GameRules gameRules = new(words);
        string data = "s";
        bool multipleLetters = false;
        int[] expected = { 2 };

        string[] letters = words[0].Select(ch => ch.ToString()).ToArray();
        letters[0] = null;
        int[] actual = gameRules.FindGuessed(data, letters, multipleLetters);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Give_Proper_Game_Progress_Data_By_Difficulty()
    {
        GameRules gameRules = new(Array.Empty<string>());
        Difficulty[] difficulties = Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>().ToArray();

        foreach (var difficulty in difficulties)
        {
            GameProgress expected = new() {
                Stage = difficulty == Difficulty.NO_CHALLANGE ? GameStageLabels.Prologue : GameStageLabels.Gameplay,
                Difficulty = DifficultyStats.StringifyDifficulty(difficulty),
                Tries = DifficultyStats.Tries(difficulty),
                Guessed = Array.Empty<string>(),
            };
            GameProgress actual = gameRules.GetInitialPlayerData(difficulty);

            Assert.AreEqual(expected, actual, message: $"Actual game progress values is invalid for {difficulty}");
        }
    }
}
