public struct LoopState : IResetable
{
    public bool Resetting;

    public bool FormatInput;

    public bool Playing;

    private TextTabsContainer _tabsContainer;

    public bool GameStarted { get => _tabsContainer.MessageVisible; }

    public bool ResetAllowed { get => !Resetting && GameStarted; }

    public bool StageExecultionAllowed { get => !Resetting && !Playing && GameStarted; }

    public LoopState(TextTabsContainer tabsContainer)
    {
        Resetting = false;
        FormatInput = false;
        Playing = false;
        _tabsContainer = tabsContainer;
    }

    public void ResetState()
    {
        Resetting = false;
        FormatInput = false;
        Playing = false;
    }
}
