using System.Collections;
using Zenject;

using Difficulty = DifficultyStats.Difficulty;

public class WinViewController : IViewController
{
    [Inject] private readonly ViewTools _viewTools;

    public IEnumerator PlayActions(GameProgress gameProgress)
    {
        var mainCharacter = _viewTools.ComponentsSpawner.MainCharacter;
        _viewTools.TextTabsContainer.Word = gameProgress.Guessed;
        string difficulty = gameProgress.Difficulty;
        if (DifficultyStats.Is(difficulty, Difficulty.NO_CHALLANGE))
            yield return mainCharacter.Move(_viewTools.CharacterMovement.Center);
        if (shouldSetBodyVisible(ref gameProgress))
        {
            mainCharacter.BodyVisible = true;
            yield return null;
        }
        mainCharacter.CurrentState = AnimationState.WIN_REACTION;
        yield return null;
    }

    private bool shouldSetBodyVisible(ref GameProgress gameProgress)
    {
        bool easyMode = DifficultyStats.Is(gameProgress.Difficulty, Difficulty.EASY);
        bool bodyVisible = gameProgress.Tries > DifficultyStats.Tries(Difficulty.NORMAL);
        return easyMode && bodyVisible;
    }
}
