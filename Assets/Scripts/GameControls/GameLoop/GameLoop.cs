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
#if UNITY_WEBGL && !UNITY_EDITOR
        // Enter submits the letter like the Submit button; it also ends editing, so the field takes focus back for the next one.
        // Focusing selects all the text by default, which made Enter look like it only highlighted the letter
        _inputField.onFocusSelectAll = false;
        _inputField.onSubmit.AddListener(async _ =>
        {
            await PlayStage();
            // An empty field plays no stage, so nothing else gives the focus back
            focusInputNextFrame();
        });
#endif
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
        // A failing stage must not leave the loop marked as playing, or every later submit is ignored
        try
        {
            bool stageInterractive;
            do stageInterractive = await runStage(input);
            while(!stageInterractive);
        }
        finally
        {
            _state.Playing = false;
#if UNITY_WEBGL && !UNITY_EDITOR
            focusInputNextFrame();
#endif
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    // Each stage focuses the field (runStage), but in the browser the click that started the stage can be
    // handled after that and take the focus away, so the field takes it back once the frame is over
    private async void focusInputNextFrame()
    {
        await UnityEngine.Awaitable.NextFrameAsync();
        if (_inputField == null || !_inputField.isActiveAndEnabled) return;
        _inputField.Select();
        _inputField.ActivateInputField();
    }
#endif

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
