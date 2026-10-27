using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using Difficulty = DifficultyStats.Difficulty;

public class GameplayViewController : IViewController, IResetable
{
    [Inject] private readonly ViewTools _viewTools;

    private const int _dialogueDuration = 2;

    private const int _noChallangeModeStartOffset = 10;

    // The head isn't listed: the last body step shows the whole body at once
    private static readonly BodyPart[] _bodyPartsAppearanceOrder = {
        BodyPart.LEFT_LEG,
        BodyPart.RIGHT_LEG,
        BodyPart.TORSO,
        BodyPart.LEFT_HAND,
        BodyPart.RIGHT_HAND,
    };

    // Hanger and body steps are counted from the tries left, so they're kept apart from the views to be checked alone

    public static bool IsLastTry(int triesLeft) => triesLeft == 1;

    public static int HangerPartIndex(int hangerParts, int triesLeft) => hangerParts - triesLeft + 1;

    public static bool ShowsHangerPartInEasyMode(int triesLeft) => triesLeft < DifficultyStats.Tries(Difficulty.NORMAL);

    // Null means the whole body is shown (the last body step)
    public static BodyPart? BodyPartToShow(int hangerParts, int triesLeft)
    {
        int offset = 2;
        int partPosition = triesLeft - hangerParts - offset;
        if (partPosition == 0) return null;
        return _bodyPartsAppearanceOrder[_bodyPartsAppearanceOrder.Length - partPosition];
    }

    public static int DisplayedTries(int tries, bool noChallangeMode)
    {
        if (!noChallangeMode) return tries;
        return tries + _noChallangeModeStartOffset - DifficultyStats.Tries(Difficulty.NO_CHALLANGE);
    }

    private readonly Dictionary<string, Func<int, IEnumerator>> _lossHandlers = new();

    private bool _initial = true;

    private int _triesPreviously;

    public GameplayViewController()
    {
        _lossHandlers[DifficultyStats.StringifyDifficulty(Difficulty.NO_CHALLANGE)] = tries => updateTriesView(tries, noChallangeMode: true);
        _lossHandlers[DifficultyStats.StringifyDifficulty(Difficulty.EASY)] = easyModeAction;
        _lossHandlers[DifficultyStats.StringifyDifficulty(Difficulty.NORMAL)] = showHangerPart;
        _lossHandlers[DifficultyStats.StringifyDifficulty(Difficulty.DIFFICULT)] = tries => updateTriesView(tries);
    }

    public void ResetState()
    {
        _initial = true;
        _triesPreviously = default;
    }

    public IEnumerator PlayActions(GameProgress gameProgress)
    {
        yield return _initial ? executeInitialActions(gameProgress) : showGuessAttempt(gameProgress);
        _triesPreviously = gameProgress.Tries;
    }

    private IEnumerator executeInitialActions(GameProgress gameProgress)
    {
        _initial = false;
        _viewTools.TextTabsContainer.Message = string.Empty;
        string difficulty = gameProgress.Difficulty;
        Func<Difficulty, bool> difficultyIs = comparable => DifficultyStats.Is(difficulty, comparable);
        bool noChallangeMode = difficultyIs(Difficulty.NO_CHALLANGE);
        if (!noChallangeMode)
        {
            if (difficultyIs(Difficulty.EASY))
                placeCharacterBody();
            else
                yield return _viewTools.ComponentsSpawner.MainCharacter.Move(_viewTools.CharacterMovement.Center);
        }
        _viewTools.TextTabsContainer.Word = gameProgress.Guessed;
        yield return updateTriesView(gameProgress.Tries, noChallangeMode);
        _viewTools.TextTabsContainer.GuessingVisible = true;
    }

    private IEnumerator showGuessAttempt(GameProgress gameProgress)
    {
        bool guessed = _triesPreviously == gameProgress.Tries;
        if (guessed) _viewTools.TextTabsContainer.Word = gameProgress.Guessed;
        else yield return _lossHandlers[gameProgress.Difficulty](gameProgress.Tries);
    }

    private IEnumerator showHangerPart(int progress)
    {
        yield return updateTriesView(progress);
        var componentsSpawner = _viewTools.ComponentsSpawner;
        if (IsLastTry(progress))
        {
            yield return null;
            yield return componentsSpawner.MainCharacter.Jump(_viewTools.CharacterMovement.Jump);
            yield break;
        }
        var hangerParts = componentsSpawner.Hanger.Parts;
        HangerPart hangerPart = hangerParts[HangerPartIndex(hangerParts.Count, progress)];
        hangerPart.StartMove();
        yield return new WaitWhile(() => hangerPart.Moving);
        yield return null;
    }

    private IEnumerator showBodyPart(int progress)
    {
        yield return updateTriesView(progress);
        var componentsSpawner = _viewTools.ComponentsSpawner;
        MainCharacter mainCharacter = componentsSpawner.MainCharacter;
        BodyPart? bodyPart = BodyPartToShow(componentsSpawner.Hanger.Parts.Count, progress);
        if (bodyPart.HasValue)
        {
            mainCharacter.SetBodyPartVisible(bodyPart.Value);
        }
        else
        {
            mainCharacter.BodyVisible = true;
            mainCharacter.CurrentState = AnimationState.STANDING;
        }
        yield return null;
    }

    private IEnumerator easyModeAction(int tries)
    {
        yield return ShowsHangerPartInEasyMode(tries) ? showHangerPart(tries) : showBodyPart(tries);
    }

    private IEnumerator updateTriesView(int tries, bool noChallangeMode = false)
    {
        _viewTools.TextTabsContainer.Tries = DisplayedTries(tries, noChallangeMode);
        yield return null;
    }

    private void placeCharacterBody()
    {
        var mainCharacter = _viewTools.ComponentsSpawner.MainCharacter;
        mainCharacter.BodyVisible = false;
        mainCharacter.transform.position = _viewTools.CharacterMovement.Center;
    }
}
