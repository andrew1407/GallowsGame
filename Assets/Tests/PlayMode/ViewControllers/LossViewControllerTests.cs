using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Zenject;

using Difficulty = DifficultyStats.Difficulty;

public class LossViewControllerTests : ZenjectIntegrationTestFixture
{
    private const string _lossMessage = "sasik";

    private TextTabsContainer.Tabs _tabs;

    void CommonInstall()
    {
        PreInstall();

        _tabs = UiFactory.MakeTabs();
        Container.BindInstances(_lossMessage, UiFactory.MakeViewTools(_tabs));
        Container.Bind<LossViewController>().AsSingle();

        PostInstall();
    }

    [UnityTest]
    public IEnumerator Should_Show_Loss_Message_And_Tries_In_No_Challange_Mode()
    {
        CommonInstall();
        var viewController = Container.Resolve<LossViewController>();
        GameProgress gameProgress = new() {
            Stage = GameStageLabels.Loss,
            Difficulty = DifficultyStats.StringifyDifficulty(Difficulty.NO_CHALLANGE),
            Tries = 0,
        };

        yield return viewController.PlayActions(gameProgress);

        Assert.AreEqual(_lossMessage, _tabs.Message.text);
        Assert.AreEqual("Tries: 0", _tabs.Tries.text);
        Assert.True(_tabs.Tries.enabled);
    }
}
