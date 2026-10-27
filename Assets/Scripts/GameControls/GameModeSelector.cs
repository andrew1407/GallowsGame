using System;
using Zenject;
using TMPro;

public class GameModeSelector
{
    [Inject] private readonly StartOptionsContainer _options;

    [Inject] private readonly GameLoop _gameLoop;

    [Inject] private readonly GameplayStrategyFactory _gameplayStrategyFactory;

    private TMP_InputField _addressInput;

    private bool _starting = false;

    public void Initialize()
    {
        _addressInput = _options.AddressField.GetComponent<TMP_InputField>();
#if UNITY_WEBGL && !UNITY_EDITOR
        // Browsers allow neither raw sockets nor HttpClient, so the web build is offline only
        _options.OfflineMode.isOn = true;
        _options.OfflineMode.interactable = false;
#endif
        _options.AddressField.SetActive(!_options.OfflineMode.isOn);
        _options.OfflineMode.onValueChanged.AddListener(toggleAction);
        _options.StartButton.onClick.AddListener(startGame);
    }

    private void toggleAction(bool value)
    {
        bool fieldVisible = !value;
        _options.AddressField.SetActive(fieldVisible);
        if (fieldVisible) _addressInput.ActivateInputField();
        else _addressInput.text = string.Empty;
    }

    private async void startGame()
    {
        if (_starting) return;
        _starting = true;
        try
        {
            IGameplayStrategy gameplayStrategy = _options.OfflineMode.isOn ? loadOfflineMode() : loadOnlineMode();
            _gameLoop.SetStrategy(gameplayStrategy);
            _options.BlockSelf.SetActive(false);
            deactivateStateParams();
            await _gameLoop.StartPlay();
        }
        catch(UriFormatException)
        {
            deactivateStateParams();
        }
    }

    private OfflineStrategy loadOfflineMode()
    {
        var resourceLoader = new ResourceLoader();
        string[] words = resourceLoader.LoadWords();
        var gameRules = new GameRules(words);
        return _gameplayStrategyFactory.MakeOfflineStrategy(gameRules);
    }

    private IGameplayStrategy loadOnlineMode()
    {
        var uri = new Uri(_addressInput.text);
        return _gameplayStrategyFactory.MakeOnlineStrategyByUri(uri);
    }

    private void deactivateStateParams()
    {
        _addressInput.text = string.Empty;
        _starting = false;
    }
}
