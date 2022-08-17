using System;
using System.Threading.Tasks;
using NUnit.Framework;

using Difficulty = DifficultyStats.Difficulty;

public static class OfflineStrategyExtensions
{
    public static GameProgress StageActionSync(this OfflineStrategy strategy, string input)
    {
        return Task.Run(() => strategy.StageAction(input)).GetAwaiter().GetResult();
    }
}

public class OfflineStrategyTests
{

    [Test]
    public void Should_Return_Proper_Default_Params()
    {
        OfflineStrategy strategy = makeTestable();
        GameProgress expected =  new() { Stage = GameStageLabels.Difficulty };

        GameProgress actual = strategy.StageActionSync(string.Empty);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Should_Return_Previous_Progress_For_Empty_Input()
    {
        OfflineStrategy strategy = makeTestable();

        GameProgress expected = strategy.StageActionSync(string.Empty);
        GameProgress actualFirst = strategy.StageActionSync(string.Empty);
        GameProgress actualSecond = strategy.StageActionSync(string.Empty);

        Assert.AreEqual(expected, actualFirst);
        Assert.AreEqual(expected, actualSecond);
    }

    [Test]
    public void Should_Return_Next_Stage_As_Prologue_For_No_Challange_Difficulty()
    {
        OfflineStrategy strategy = makeTestable(word: "bobo");
        string difficultyInput = "4";
        string expected = GameStageLabels.Prologue;

        GameProgress actual = strategy.StageActionSync(difficultyInput);

        Assert.AreEqual(expected, actual.Stage);
    }

    [Test]
    public void Should_Return_Next_Stage_As_Gameplay_After_Difficulty_Stage()
    {
        string[] difficulties = { "1", "2", "3" };
        string expected = GameStageLabels.Gameplay;

        foreach (var difficulty in difficulties)
        {
            OfflineStrategy strategy = makeTestable(word: "bobo");
            GameProgress actual = strategy.StageActionSync(difficulty);
            Assert.AreEqual(expected, actual.Stage, message: $"Invalid stage for {difficulty}");
        }
    }

    [Test]
    public void Should_Return_Null_After_Win_Stage()
    {
        OfflineStrategy strategy = makeTestable(word: "bobo");

        strategy.StageActionSync("1");
        strategy.StageActionSync("b");
        strategy.StageActionSync("o");
        GameProgress gameProgress = strategy.StageActionSync(string.Empty);

        Assert.IsNull(gameProgress.Stage);
    }

    [Test]
    public void Should_Return_Null_After_Loss_Stage()
    {
        OfflineStrategy strategy = makeTestable(word: "bobo");

        strategy.StageActionSync("1");
        strategy.StageActionSync("a");
        strategy.StageActionSync("a");
        strategy.StageActionSync("a");
        strategy.StageActionSync("a");
        GameProgress gameProgress = strategy.StageActionSync(string.Empty);

        Assert.IsNull(gameProgress.Stage);
    }

    [Test]
    public void Should_Return_Guessed_Case_Result()
    {
        OfflineStrategy strategy = makeTestable(word: "bobo");
        GameProgress expectedFirts = new() {
            Difficulty = (Difficulty.NORMAL as Enum).ToString(),
            Stage = GameStageLabels.Gameplay,
            Tries = 7,
            Guessed = new string[] { "b", null, "b", null },
        };
        GameProgress expectedSecond = new() {
            Difficulty = (Difficulty.NORMAL as Enum).ToString(),
            Stage = GameStageLabels.Gameplay,
            Tries = 6,
            Guessed = new string[] { "b", "o", "b", "o" },
        };

        strategy.StageActionSync("2");

        GameProgress resultFirst = strategy.StageActionSync("b");
        Assert.AreEqual(expectedFirts.Guessed, resultFirst.Guessed);
        Assert.AreEqual(expectedFirts.Tries, resultFirst.Tries);

        strategy.StageActionSync("b");

        GameProgress resultSecond = strategy.StageActionSync("o");
        Assert.AreEqual(expectedSecond.Guessed, resultSecond.Guessed);
        Assert.AreEqual(expectedSecond.Tries, resultSecond.Tries);
    }

    [Test]
    public void Should_Return_Not_Guessed_Case_Result()
    {
        OfflineStrategy strategy = makeTestable(word: "bobo");
        strategy.StageActionSync("2");
        strategy.StageActionSync("b");
        for (int tries = 6; tries > 0; --tries)
        {
            GameProgress actual = strategy.StageActionSync("b");
            Assert.AreEqual(tries, actual.Tries);
        }
    }

    private OfflineStrategy makeTestable(string word = null)
    {
        string[] words = string.IsNullOrEmpty(word) ? Array.Empty<string>() : new[] { word };
        var gameRules = new GameRules(words);
        var offlineMode = new OfflineStrategy(gameRules);
        Task.Run(offlineMode.Setup);
        return offlineMode;
    }
}
