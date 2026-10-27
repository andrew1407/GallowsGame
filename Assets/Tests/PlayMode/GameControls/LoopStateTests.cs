using NUnit.Framework;

public class LoopStateTests
{
    private TextTabsContainer.Tabs _tabs;

    private TextTabsContainer _tabsContainer;

    [SetUp]
    public void SetUp()
    {
        _tabs = UiFactory.MakeTabs();
        _tabsContainer = new TextTabsContainer(_tabs);
    }

    [TearDown]
    public void TearDown() => UiFactory.Destroy(_tabs);

    [Test]
    public void Should_Treat_Game_As_Started_While_Message_Is_Visible()
    {
        LoopState state = new(_tabsContainer);

        _tabsContainer.MessageVisible = false;
        Assert.False(state.GameStarted);
        Assert.False(state.ResetAllowed, message: "Reset isn't allowed before the game starts");
        Assert.False(state.StageExecultionAllowed, message: "Stages can't run before the game starts");

        _tabsContainer.MessageVisible = true;
        Assert.True(state.GameStarted);
        Assert.True(state.ResetAllowed);
        Assert.True(state.StageExecultionAllowed);
    }

    [Test]
    public void Should_Block_Reset_And_Stages_While_Resetting()
    {
        LoopState state = new(_tabsContainer);
        _tabsContainer.MessageVisible = true;

        state.Resetting = true;

        Assert.False(state.ResetAllowed);
        Assert.False(state.StageExecultionAllowed);
    }

    [Test]
    public void Should_Block_Only_Stages_While_Playing()
    {
        LoopState state = new(_tabsContainer);
        _tabsContainer.MessageVisible = true;

        state.Playing = true;

        Assert.True(state.ResetAllowed, message: "Reset should interrupt a playing stage");
        Assert.False(state.StageExecultionAllowed);
    }

    [Test]
    public void Should_Clear_Flags_On_Reset()
    {
        LoopState state = new(_tabsContainer) { Resetting = true, Playing = true, FormatInput = true };

        state.ResetState();

        Assert.False(state.Resetting);
        Assert.False(state.Playing);
        Assert.False(state.FormatInput);
    }
}
