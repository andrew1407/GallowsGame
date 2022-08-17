public struct ViewTools : IResetable
{
    public ComponentsSpawner ComponentsSpawner;

    public CharacterMovement CharacterMovement;

    public TextTabsContainer TextTabsContainer;

    public void Initialize()
    {
        ComponentsSpawner.SpawnMainCharacter();
        ComponentsSpawner.SpawnHanger();
        ComponentsSpawner.SpawnRunner();
        
        TextTabsContainer.CharactedDialogueVisible = false;
        TextTabsContainer.RunnerDialogueVisible = false;
        TextTabsContainer.GuessingVisible = false;
        TextTabsContainer.MessageVisible = false;
    }

    public void ResetState()
    {
        ComponentsSpawner.ResetState();
        TextTabsContainer.ResetState();
    }
}
