using System.Collections;
using UnityEngine;
using Zenject;

public class PrologueViewController : IViewController
{
    [Inject] private readonly ViewTools _viewTools;

    private const int _dialogueDuration = 5;

    public IEnumerator PlayActions(GameProgress gameProgress)
    {
        var mainCharacter = _viewTools.ComponentsSpawner.MainCharacter;
        var tabsContainer = _viewTools.TextTabsContainer;
        var destinations = _viewTools.CharacterMovement;
        
        tabsContainer.Message = string.Empty;
        yield return mainCharacter.Move(destinations.Center);
        mainCharacter.CurrentState = AnimationState.TALKING;
        yield return null;
        tabsContainer.CharactedDialogueVisible = true;
        yield return new WaitForSeconds(_dialogueDuration);
        tabsContainer.CharactedDialogueVisible = false;
        yield return null;
        yield return mainCharacter.Move(destinations.OutOfBounds);
        mainCharacter.ResetState();
    }
}
