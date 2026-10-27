using NUnit.Framework;

public class GameStageLabelsTests
{
    [Test]
    public void Should_Treat_Difficulty_And_Gameplay_As_Interractive()
    {
        Assert.True(GameStageLabels.IsInterractive(GameStageLabels.Difficulty));
        Assert.True(GameStageLabels.IsInterractive(GameStageLabels.Gameplay));
    }

    [Test]
    public void Should_Treat_Other_Stages_As_Not_Interractive()
    {
        string[] stages = { GameStageLabels.Prologue, GameStageLabels.Win, GameStageLabels.Loss, null, string.Empty, "sasik" };

        foreach (var stage in stages)
            Assert.False(GameStageLabels.IsInterractive(stage), message: $"Stage \"{stage}\" shouldn't be interractive");
    }
}
