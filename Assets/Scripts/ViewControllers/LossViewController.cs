using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using Difficulty = DifficultyStats.Difficulty;

public class LossViewController : IViewController
{
    [Inject] private readonly ViewTools _viewTools;

    [Inject] private readonly string _messageText;

    private const int _runnerDialogueDuration = 2;

    private readonly Dictionary<string, Func<IEnumerator>> _lossHandlers = new();

    public LossViewController()
    {
        _lossHandlers[DifficultyStats.StringifyDifficulty(Difficulty.NO_CHALLANGE)] = showMessage;
        _lossHandlers[DifficultyStats.StringifyDifficulty(Difficulty.EASY)] = hangCharacter;
        _lossHandlers[DifficultyStats.StringifyDifficulty(Difficulty.NORMAL)] = hangCharacter;
        _lossHandlers[DifficultyStats.StringifyDifficulty(Difficulty.DIFFICULT)] = activateRunner;
    }

    public IEnumerator PlayActions(GameProgress gameProgress)
    {
        _viewTools.TextTabsContainer.Tries = gameProgress.Tries;
        yield return _lossHandlers[gameProgress.Difficulty]();
    }

    private IEnumerator hangCharacter()
    {
        var mainCharacter = _viewTools.ComponentsSpawner.MainCharacter;
        yield return mainCharacter.Jump(_viewTools.CharacterMovement.SecondJump);
        mainCharacter.CurrentState = AnimationState.HANGED;
    }

    private IEnumerator showMessage()
    {
        _viewTools.TextTabsContainer.Message = _messageText;
        yield return null;
    }

    private IEnumerator activateRunner()
    {
        var tabsContainer = _viewTools.TextTabsContainer;
        var componentsSpawner = _viewTools.ComponentsSpawner;
        tabsContainer.RunnerDialogueVisible = true;
        yield return new WaitForSeconds(_runnerDialogueDuration);
        tabsContainer.RunnerDialogueVisible = false;
        yield return null;
        componentsSpawner.Runner.StartRun();
        yield return new WaitWhile(() => componentsSpawner.Runner.Running);
    }
}
