using System;
using System.Threading.Tasks;
using TMPro;
using Zenject;

public class GameLoop : IResetable, IDisposable
{
    [Inject] private readonly ViewTools _viewTools;

    [Inject] private readonly TMP_InputField _inputField;

    [Inject] private readonly StageActivity _stageActivity;

    [Inject] private readonly ViewsContainer _viewsContainer;

    private LoopState _state;

    private IGameplayStrategy _gameplayStrategy;

    public void Initialize()
    {
        _state = new(_viewTools.TextTabsContainer);
        _inputField.onValueChanged.AddListener(formatInput);
        _viewTools.Initialize();
    }

    public void SetStrategy(IGameplayStrategy strategy) => _gameplayStrategy = strategy;

    public async void ResetState()
    {
        if (!_state.ResetAllowed) return;
        _state.Resetting = true;
        if (_state.Playing) _stageActivity.ResetState();
        _viewsContainer.ResetState();
        if (_gameplayStrategy is IResetable resetable) resetable.ResetState();
        _viewTools.ResetState();
        _state.ResetState();

        await StartPlay();
    }

    public void Dispose()
    {
        if (_gameplayStrategy is IDisposable disposable) disposable.Dispose();
    }

    public async Task StartPlay()
    {
        _viewTools.TextTabsContainer.MessageVisible = true;
        _viewTools.TextTabsContainer.GuessingVisible = false;
        await _gameplayStrategy.Setup();
        await playActions(string.Empty);
    }

    public async Task PlayStage()
    {
        if (!_state.StageExecultionAllowed) return;
        string input = _inputField.text;
        if (string.IsNullOrEmpty(input)) return;
        await playActions(input);
    }

    private void formatInput(string input)
    {
        if (!_state.FormatInput) return;
        if (input.Length > 1)
            _inputField.text = input[input.Length - 1].ToString();
    }

    private async Task playActions(string input)
    {
        if (_state.Playing) return;
        _state.Playing = true;
        bool stageInterractive;
        do stageInterractive = await runStage(input);
        while(!stageInterractive);
        _state.Playing = false;
    }

    private async Task<bool> runStage(string input)
    {
        var gameProgress = await _gameplayStrategy.StageAction(input);
        string stage = gameProgress.Stage;
        _state.FormatInput = stage != GameStageLabels.Difficulty;
        _inputField.text = string.Empty;
        _inputField.ActivateInputField();
        if (string.IsNullOrEmpty(stage)) return true;
        IViewController view = _viewsContainer[stage];
        bool success = await _stageActivity.AwaitActionFinish(view.PlayActions(gameProgress));
        if (!success) return true;
        return GameStageLabels.IsInterractive(stage);
    }
}
