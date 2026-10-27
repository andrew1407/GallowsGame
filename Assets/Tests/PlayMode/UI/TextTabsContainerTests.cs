using NUnit.Framework;

public class TextTabsContainerTests
{
    private TextTabsContainer.Tabs _tabs;

    [SetUp]
    public void SetUp() => _tabs = UiFactory.MakeTabs();

    [TearDown]
    public void TearDown() => UiFactory.Destroy(_tabs);

    [Test]
    public void Should_Set_Message_Text_And_Visibility()
    {
        TextTabsContainer tabsContainer = new(_tabs);

        tabsContainer.Message = "sasik";
        tabsContainer.MessageVisible = false;

        Assert.AreEqual("sasik", _tabs.Message.text);
        Assert.False(_tabs.Message.enabled);
        Assert.False(tabsContainer.MessageVisible);

        tabsContainer.MessageVisible = true;
        Assert.True(tabsContainer.MessageVisible);
    }

    [Test]
    public void Should_Format_Tries_And_Show_Them()
    {
        TextTabsContainer tabsContainer = new(_tabs);
        _tabs.Tries.enabled = false;

        tabsContainer.Tries = 7;

        Assert.AreEqual("Tries: 7", _tabs.Tries.text);
        Assert.True(_tabs.Tries.enabled, message: "Tries tab should become visible when tries are set");
    }

    [Test]
    public void Should_Format_Word_With_Unknown_Letters()
    {
        TextTabsContainer tabsContainer = new(_tabs);

        tabsContainer.Word = new[] { "b", null, "b", string.Empty };

        Assert.AreEqual("| b | ? | b | ? |", _tabs.Word.text);
    }

    [Test]
    public void Should_Toggle_Tries_And_Word_Together()
    {
        TextTabsContainer tabsContainer = new(_tabs);

        tabsContainer.GuessingVisible = false;
        Assert.False(_tabs.Tries.enabled);
        Assert.False(_tabs.Word.enabled);

        tabsContainer.GuessingVisible = true;
        Assert.True(_tabs.Tries.enabled);
        Assert.True(_tabs.Word.enabled);
    }

    [Test]
    public void Should_Toggle_Dialogues()
    {
        TextTabsContainer tabsContainer = new(_tabs);

        tabsContainer.CharactedDialogueVisible = true;
        tabsContainer.RunnerDialogueVisible = false;
        Assert.True(_tabs.CharactedDialogue.enabled);
        Assert.False(_tabs.RunnerDialogue.enabled);

        tabsContainer.CharactedDialogueVisible = false;
        tabsContainer.RunnerDialogueVisible = true;
        Assert.False(_tabs.CharactedDialogue.enabled);
        Assert.True(_tabs.RunnerDialogue.enabled);
    }

    [Test]
    public void Should_Clear_Texts_And_Hide_Tabs_On_Reset()
    {
        TextTabsContainer tabsContainer = new(_tabs);
        tabsContainer.Message = "sasik";
        tabsContainer.Tries = 3;
        tabsContainer.Word = new[] { "b", "o" };
        tabsContainer.GuessingVisible = true;
        tabsContainer.CharactedDialogueVisible = true;
        tabsContainer.RunnerDialogueVisible = true;

        tabsContainer.ResetState();

        Assert.IsEmpty(_tabs.Message.text);
        Assert.IsEmpty(_tabs.Word.text);
        Assert.False(_tabs.Tries.enabled);
        Assert.False(_tabs.Word.enabled);
        Assert.False(_tabs.CharactedDialogue.enabled);
        Assert.False(_tabs.RunnerDialogue.enabled);
    }
}
