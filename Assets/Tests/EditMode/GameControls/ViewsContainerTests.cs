using System;
using System.Collections.Generic;
using NUnit.Framework;
using Zenject;

public class ViewsContainerTests : ZenjectUnitTestFixture
{
    [SetUp]
    public void CommonInstall()
    {
        Container.BindInstance(new ViewTools());
        Container.BindInstance("sasik");
        Container.Bind<IViewController>().To<DifficultyViewController>().AsSingle();
        Container.Bind<IViewController>().To<PrologueViewController>().AsSingle();
        Container.Bind<IViewController>().To<GameplayViewController>().AsSingle();
        Container.Bind<IViewController>().To<LossViewController>().AsSingle();
        Container.Bind<IViewController>().To<WinViewController>().AsSingle();
        Container.Bind<ViewsContainer>().AsSingle();
    }

    [Test]
    public void Should_Map_Every_Stage_To_Its_View_Controller()
    {
        var viewsContainer = Container.Resolve<ViewsContainer>();
        Dictionary<string, Type> expected = new() {
            {GameStageLabels.Difficulty, typeof(DifficultyViewController)},
            {GameStageLabels.Prologue, typeof(PrologueViewController)},
            {GameStageLabels.Gameplay, typeof(GameplayViewController)},
            {GameStageLabels.Loss, typeof(LossViewController)},
            {GameStageLabels.Win, typeof(WinViewController)},
        };

        foreach (var stage in expected)
            Assert.IsInstanceOf(stage.Value, viewsContainer[stage.Key], message: $"Invalid view controller for \"{stage.Key}\" stage");
    }

    [Test]
    public void Should_Reuse_Bound_View_Controller_Instances()
    {
        var viewsContainer = Container.Resolve<ViewsContainer>();
        var controllers = Container.ResolveAll<IViewController>();

        foreach (var stage in new[] { GameStageLabels.Difficulty, GameStageLabels.Win })
            Assert.Contains(viewsContainer[stage], controllers);
    }

    [Test]
    public void Should_Throw_For_Unknown_Stage()
    {
        var viewsContainer = Container.Resolve<ViewsContainer>();

        Assert.Throws<KeyNotFoundException>(() => _ = viewsContainer["sasik"]);
    }

    [Test]
    public void Should_Reset_Resetable_View_Controllers()
    {
        var viewsContainer = Container.Resolve<ViewsContainer>();

        Assert.DoesNotThrow(viewsContainer.ResetState);
    }
}
