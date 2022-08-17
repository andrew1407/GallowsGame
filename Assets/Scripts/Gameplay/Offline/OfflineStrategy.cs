using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

public class OfflineStrategy : IGameplayStrategy
{
    private readonly GameRules _gameRules;

    private readonly Dictionary<string, Action<string>> _stageHandlers = new();

    private GameProgress _gameProgress = default;

    private bool _multipleLetters;

    private string[] _lettersLeft;

    public OfflineStrategy(GameRules gameRules)
    {
        _gameRules = gameRules;
        Func<string, Action<string>> setLebel = label => _ => _gameProgress.Stage = label;
        _stageHandlers[GameStageLabels.Difficulty] = onDifficultyStage;
        _stageHandlers[GameStageLabels.Prologue] = setLebel(GameStageLabels.Gameplay);
        _stageHandlers[GameStageLabels.Gameplay] = guessWord;
        _stageHandlers[GameStageLabels.Win] = setLebel(null);
        _stageHandlers[GameStageLabels.Loss] = setLebel(null);
    }

    public Task Setup()
    {
        _gameProgress = new() { Stage = GameStageLabels.Difficulty };
        _multipleLetters = default;
        _lettersLeft = Array.Empty<string>();
        return Task.CompletedTask;
    }

    public Task<GameProgress> StageAction(string input)
    {
        string stage = _gameProgress.Stage;
        bool canHandle = !string.IsNullOrEmpty(stage) && _stageHandlers.ContainsKey(stage);
        if (canHandle) _stageHandlers[stage](input);
        return Task.FromResult(_gameProgress);
    }

    private void onDifficultyStage(string input)
    {
        try
        {
            var difficulty = _gameRules.ParseDifficulty(input, out _multipleLetters);
            _gameProgress = _gameRules.GetInitialPlayerData(difficulty);
            _lettersLeft = _gameRules.PickWord(out _gameProgress.Guessed);
        }
        catch (Exception e)
        {
            if (e is not InvalidDataException) throw e;
        }
    }

    private void guessWord(string input)
    {
        int[] indicesFound = _gameRules.FindGuessed(input, _lettersLeft, _multipleLetters);
        if (indicesFound.Length == 0)
        {
            if (--_gameProgress.Tries == 0) _gameProgress.Stage = GameStageLabels.Loss;
            return;
        }
        foreach (var i in indicesFound)
        {
            _gameProgress.Guessed[i] = _lettersLeft[i];
            _lettersLeft[i] = null;
        }
        bool allLettersGuessed = Array.IndexOf(_gameProgress.Guessed, null) == -1;
        if (allLettersGuessed) _gameProgress.Stage = GameStageLabels.Win;
    }
}
