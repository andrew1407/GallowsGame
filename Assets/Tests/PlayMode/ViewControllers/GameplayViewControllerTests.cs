using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Zenject;

using Difficulty = DifficultyStats.Difficulty;

// NO_CHALLANGE mode plays without the character walking in, so it covers the UI part of the gameplay stage alone
public class GameplayViewControllerTests : ZenjectIntegrationTestFixture
{
    private static readonly string _noChallange = DifficultyStats.StringifyDifficulty(Difficulty.NO_CHALLANGE);

    private static readonly int _noChallangeTries = DifficultyStats.Tries(Difficulty.NO_CHALLANGE);

    private TextTabsContainer.Tabs _tabs;

    void CommonInstall()
    {
        PreInstall();

        _tabs = UiFactory.MakeTabs();
        Container.BindInstance(UiFactory.MakeViewTools(_tabs));
        Container.Bind<GameplayViewController>().AsSingle();

        PostInstall();
    }

    [UnityTest]
    public IEnumerator Should_Show_Word_And_Tries_On_Initial_Stage()
    {
        CommonInstall();
        var viewController = Container.Resolve<GameplayViewController>();
        _tabs.Message.text = "difficulties";
        _tabs.Word.enabled = _tabs.Tries.enabled = false;

        yield return viewController.PlayActions(progress(_noChallangeTries, null, null, null));

        Assert.IsEmpty(_tabs.Message.text, message: "Difficulties description should be cleared");
        Assert.AreEqual("| ? | ? | ? |", _tabs.Word.text);
        Assert.AreEqual("Tries: 10", _tabs.Tries.text, message: "No challange mode should count tries down from 10");
        Assert.True(_tabs.Word.enabled, message: "Word should be visible");
        Assert.True(_tabs.Tries.enabled, message: "Tries should be visible");
    }

    [UnityTest]
    public IEnumerator Should_Reveal_Guessed_Letters_Without_Changing_Tries()
    {
        CommonInstall();
        var viewController = Container.Resolve<GameplayViewController>();

        yield return viewController.PlayActions(progress(_noChallangeTries, null, null, null));
        yield return viewController.PlayActions(progress(_noChallangeTries, "b", null, "b"));

        Assert.AreEqual("| b | ? | b |", _tabs.Word.text);
        Assert.AreEqual("Tries: 10", _tabs.Tries.text);
    }

    [UnityTest]
    public IEnumerator Should_Decrease_Tries_On_Wrong_Guess()
    {
        CommonInstall();
        var viewController = Container.Resolve<GameplayViewController>();

        yield return viewController.PlayActions(progress(_noChallangeTries, null, null));
        yield return viewController.PlayActions(progress(_noChallangeTries - 1, null, null));
        yield return viewController.PlayActions(progress(_noChallangeTries - 2, null, null));

        Assert.AreEqual("Tries: 8", _tabs.Tries.text);
        Assert.AreEqual("| ? | ? |", _tabs.Word.text, message: "Wrong guess shouldn't change the word");
    }

    [UnityTest]
    public IEnumerator Should_Start_From_Initial_Stage_After_Reset()
    {
        CommonInstall();
        var viewController = Container.Resolve<GameplayViewController>();

        yield return viewController.PlayActions(progress(_noChallangeTries - 3, "a"));
        viewController.ResetState();
        _tabs.Message.text = "difficulties";
        yield return viewController.PlayActions(progress(_noChallangeTries, null, null));

        Assert.IsEmpty(_tabs.Message.text, message: "Initial actions should run again after reset");
        Assert.AreEqual("| ? | ? |", _tabs.Word.text);
        Assert.AreEqual("Tries: 10", _tabs.Tries.text);
    }

    private static GameProgress progress(int tries, params string[] guessed) => new() {
        Stage = GameStageLabels.Gameplay,
        Difficulty = _noChallange,
        Tries = tries,
        Guessed = guessed,
    };
}
