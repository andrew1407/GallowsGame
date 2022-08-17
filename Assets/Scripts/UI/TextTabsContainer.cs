using System;
using System.Linq;
using TMPro;

public class TextTabsContainer : IResetable
{
    [Serializable]
    public struct Tabs
    {
        public TextMeshProUGUI Message;

        public TextMeshProUGUI Tries;

        public TextMeshProUGUI Word;

        public TextMeshProUGUI CharactedDialogue;
        
        public TextMeshProUGUI RunnerDialogue;
    }

    private readonly Tabs _tabs;

    public bool CharactedDialogueVisible { set => _tabs.CharactedDialogue.enabled = value; }

    public bool RunnerDialogueVisible { set => _tabs.RunnerDialogue.enabled = value; }

    public bool MessageVisible
    {
        get => _tabs.Message.enabled;
        set => _tabs.Message.enabled = value;
    }

    public string Message { set => _tabs.Message.text = value; }

    public int Tries {
        set
        {
            TextMeshProUGUI tries = _tabs.Tries;
            if (!tries.enabled) tries.enabled = true;
            tries.text = "Tries: " + value;
        }
    }

    public string[] Word
    {
        set
        {
            var formatted = value.Select(ch => string.IsNullOrEmpty(ch) ? "?" : ch);
            _tabs.Word.text = "| " + string.Join(" | ", formatted) + " |";
        }
    }

    public bool GuessingVisible { set => _tabs.Tries.enabled =_tabs.Word.enabled = value; }

    public TextTabsContainer(Tabs tabs) => _tabs = tabs;

    public void ResetState()
    {
        CharactedDialogueVisible = false;
        RunnerDialogueVisible = false;
        _tabs.Tries.enabled = false;
        Message = string.Empty;
        _tabs.Word.text = string.Empty;
        GuessingVisible = false;
    }
}
