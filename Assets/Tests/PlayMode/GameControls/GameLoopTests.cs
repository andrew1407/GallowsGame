using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine.TestTools;
using Zenject;

using Difficulty = DifficultyStats.Difficulty;

public class GameLoopTests : ZenjectIntegrationTestFixture
{
    private static readonly string _noChallange = DifficultyStats.StringifyDifficulty(Difficulty.NO_CHALLANGE);

    private TextTabsContainer.Tabs _tabs;

    private TMP_InputField _inputField;

    private ScriptedStrategy _strategy;

    private GameLoop _gameLoop;

    void CommonInstall()
    {
        PreInstall();

        _tabs = UiFactory.MakeTabs();
        _inputField = UiFactory.MakeInputField("GameInput");
        GameLoopBindings.Install(Container, _tabs, _inputField);

        PostInstall();

        _strategy = new ScriptedStrategy();
        _gameLoop = Container.Resolve<GameLoop>();
        _gameLoop.Initialize();
        _gameLoop.SetStrategy(_strategy);
    }

    [UnityTest]
    public IEnumerator Should_Hide_Game_UI_On_Initialize()
    {
        CommonInstall();
        yield return null;

        Assert.False(_tabs.Message.enabled);
        Assert.False(_tabs.Word.enabled);
        Assert.False(_tabs.Tries.enabled);
        Assert.False(_tabs.CharactedDialogue.enabled);
        Assert.False(_tabs.RunnerDialogue.enabled);
    }

    [UnityTest]
    public IEnumerator Should_Show_Difficulties_On_Start()
    {
        CommonInstall();
        _strategy.Stages.Enqueue(stage(GameStageLabels.Difficulty));

        yield return Wait.For(_gameLoop.StartPlay());

        Assert.AreEqual(1, _strategy.SetupCalls, message: "Strategy should be set up on start");
        Assert.AreEqual(new[] { string.Empty }, _strategy.Inputs.ToArray(), message: "First stage is requested without input");
        Assert.True(_tabs.Message.enabled);
        Assert.False(_tabs.Word.enabled);
        Assert.AreEqual(GameLoopBindings.DifficultyText, _tabs.Message.text);
    }

    [UnityTest]
    public IEnumerator Should_Ignore_Stages_Before_Start()
    {
        CommonInstall();

        _inputField.text = "1";
        yield return Wait.For(_gameLoop.PlayStage());

        Assert.IsEmpty(_strategy.Inputs);
    }

    [UnityTest]
    public IEnumerator Should_Ignore_Empty_Input()
    {
        CommonInstall();
        _strategy.Stages.Enqueue(stage(GameStageLabels.Difficulty));
        yield return Wait.For(_gameLoop.StartPlay());

        _inputField.text = string.Empty;
        yield return Wait.For(_gameLoop.PlayStage());

        Assert.AreEqual(1, _strategy.Inputs.Count);
    }

    [UnityTest]
    public IEnumerator Should_Pass_Input_And_Stop_On_Interractive_Stage()
    {
        CommonInstall();
        _strategy.Stages.Enqueue(stage(GameStageLabels.Difficulty));
        _strategy.Stages.Enqueue(gameplay(DifficultyStats.Tries(Difficulty.NO_CHALLANGE), null, null));
        yield return Wait.For(_gameLoop.StartPlay());

        _inputField.text = "4";
        yield return Wait.For(_gameLoop.PlayStage());

        Assert.AreEqual(new[] { string.Empty, "4" }, _strategy.Inputs.ToArray());
        Assert.IsEmpty(_inputField.text, message: "Input should be cleared after a stage");
        Assert.AreEqual("| ? | ? |", _tabs.Word.text);
        Assert.True(_tabs.Word.enabled);
    }

    [UnityTest]
    public IEnumerator Should_Keep_Playing_Until_Interractive_Or_Last_Stage()
    {
        CommonInstall();
        _strategy.Stages.Enqueue(stage(GameStageLabels.Difficulty));
        _strategy.Stages.Enqueue(new() { Stage = GameStageLabels.Loss, Difficulty = _noChallange, Tries = 0 });
        _strategy.Stages.Enqueue(new() { Stage = null });
        yield return Wait.For(_gameLoop.StartPlay());

        _inputField.text = "b";
        yield return Wait.For(_gameLoop.PlayStage());

        Assert.AreEqual(new[] { string.Empty, "b", "b" }, _strategy.Inputs.ToArray(), message: "Loss stage isn't interractive, so the next one is requested right away");
        Assert.AreEqual(GameLoopBindings.LossText, _tabs.Message.text);
    }

    [UnityTest]
    public IEnumerator Should_Keep_Whole_Input_For_Difficulty()
    {
        CommonInstall();
        _strategy.Stages.Enqueue(stage(GameStageLabels.Difficulty));
        yield return Wait.For(_gameLoop.StartPlay());

        _inputField.text = "1++";

        Assert.AreEqual("1++", _inputField.text, message: "Difficulty input can be longer than one letter");
    }

    [UnityTest]
    public IEnumerator Should_Keep_Only_Last_Letter_During_Gameplay()
    {
        CommonInstall();
        _strategy.Stages.Enqueue(stage(GameStageLabels.Difficulty));
        _strategy.Stages.Enqueue(gameplay(DifficultyStats.Tries(Difficulty.NO_CHALLANGE), new string[] { null }));
        yield return Wait.For(_gameLoop.StartPlay());
        _inputField.text = "4";
        yield return Wait.For(_gameLoop.PlayStage());

        _inputField.text = "abc";

        Assert.AreEqual("c", _inputField.text);
    }

    [UnityTest]
    public IEnumerator Should_Restart_Game_On_Reset()
    {
        CommonInstall();
        _strategy.Stages.Enqueue(stage(GameStageLabels.Difficulty));
        _strategy.Stages.Enqueue(stage(GameStageLabels.Difficulty));
        yield return Wait.For(_gameLoop.StartPlay());
        _tabs.Message.text = "sasik";

        _gameLoop.ResetState();
        yield return Wait.Until(() => _strategy.Inputs.Count == 2, "Game wasn't restarted");

        Assert.AreEqual(1, _strategy.ResetCalls, message: "Strategy should be reset");
        Assert.AreEqual(2, _strategy.SetupCalls, message: "Strategy should be set up again");
        Assert.AreEqual(GameLoopBindings.DifficultyText, _tabs.Message.text, message: "Difficulties should be shown again");
    }

    [UnityTest]
    public IEnumerator Should_Ignore_Reset_Before_Start()
    {
        CommonInstall();

        _gameLoop.ResetState();
        yield return null;

        Assert.AreEqual(0, _strategy.ResetCalls);
        Assert.AreEqual(0, _strategy.SetupCalls);
    }

    [UnityTest]
    public IEnumerator Should_Dispose_Strategy()
    {
        CommonInstall();

        _gameLoop.Dispose();
        yield return null;

        Assert.AreEqual(1, _strategy.DisposeCalls);
    }

    private static GameProgress stage(string label) => new() { Stage = label };

    private static GameProgress gameplay(int tries, params string[] guessed) => new() {
        Stage = GameStageLabels.Gameplay,
        Difficulty = _noChallange,
        Tries = tries,
        Guessed = guessed,
    };
}
