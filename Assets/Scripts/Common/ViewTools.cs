public struct ViewTools : IResetable
{
    public ComponentsSpawner ComponentsSpawner;

    public CharacterMovement CharacterMovement;

    public TextTabsContainer TextTabsContainer;

    // UI-only setups (such as tests) have no spawner, so there are no characters to spawn or reset
    private bool _hasSpawner => ComponentsSpawner != null;

    public void Initialize()
    {
        if (_hasSpawner)
        {
            ComponentsSpawner.SpawnMainCharacter();
            ComponentsSpawner.SpawnHanger();
            ComponentsSpawner.SpawnRunner();
        }

        TextTabsContainer.CharactedDialogueVisible = false;
        TextTabsContainer.RunnerDialogueVisible = false;
        TextTabsContainer.GuessingVisible = false;
        TextTabsContainer.MessageVisible = false;
    }

    public void ResetState()
    {
        if (_hasSpawner) ComponentsSpawner.ResetState();
        TextTabsContainer.ResetState();
    }
}
