using System;
using System.Collections.Generic;
using NUnit.Framework;

using Difficulty = DifficultyStats.Difficulty;

public class DifficultyStatsTests
{
    [Test]
    public void Should_Stringify_Difficulty_As_Enum_Name()
    {
        foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
        {
            string expected = Enum.GetName(typeof(Difficulty), difficulty);
            string actual = DifficultyStats.StringifyDifficulty(difficulty);
            Assert.AreEqual(expected, actual, message: $"Invalid string value for {difficulty}");
        }
    }

    [Test]
    public void Should_Compare_Stringified_Difficulty()
    {
        string value = DifficultyStats.StringifyDifficulty(Difficulty.EASY);

        Assert.True(DifficultyStats.Is(value, Difficulty.EASY));
        Assert.False(DifficultyStats.Is(value, Difficulty.NORMAL));
        Assert.False(DifficultyStats.Is(null, Difficulty.EASY));
    }

    [Test]
    public void Should_Give_Same_Tries_By_Enum_And_By_Name()
    {
        foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
        {
            int expected = DifficultyStats.Tries(difficulty);
            int actual = DifficultyStats.Tries(DifficultyStats.StringifyDifficulty(difficulty));
            Assert.AreEqual(expected, actual, message: $"Tries mismatch for {difficulty}");
        }
    }

    [Test]
    public void Should_Give_More_Tries_For_Easier_Difficulties()
    {
        Difficulty[] fromHardest = { Difficulty.DIFFICULT, Difficulty.NORMAL, Difficulty.EASY, Difficulty.NO_CHALLANGE };

        for (int i = 1; i < fromHardest.Length; ++i)
        {
            int harder = DifficultyStats.Tries(fromHardest[i - 1]);
            int easier = DifficultyStats.Tries(fromHardest[i]);
            Assert.Greater(easier, harder, message: $"{fromHardest[i]} should have more tries than {fromHardest[i - 1]}");
        }
    }

    [Test]
    public void Should_Throw_For_Unknown_Difficulty_Name()
    {
        Assert.Throws<KeyNotFoundException>(() => DifficultyStats.Tries("sasik"));
    }
}
