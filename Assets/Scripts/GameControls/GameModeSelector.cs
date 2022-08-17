using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Zenject;
using TMPro;

public class GameModeSelector
{
    [Inject] private readonly StartOptionsContainer _options;

    [Inject] private readonly GameLoop _gameLoop;

    private TMP_InputField _addressInput;

    private bool _starting = false;

    public void Initialize()
    {
        _addressInput = _options.AddressField.GetComponent<TMP_InputField>();
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
            IGameplayStrategy gameplayStrategy = _options.OfflineMode.isOn ? await loadOfflineMode() : loadOnlineMode();
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

    private async Task<OfflineStrategy> loadOfflineMode()
    {
        var resourceLoader = new ResourceLoader();
        string[] words = await resourceLoader.LoadWords();
        var gameRules = new GameRules(words);
        return new(gameRules);
    }

    private IGameplayStrategy loadOnlineMode()
    {
        var uri = new Uri(_addressInput.text);
        Dictionary<string, Func<IGameplayStrategy>> strategies = new() {
            {"http", () => new HttpClientStrategy(new HttpClient(), uri)},
            {"ws", () => new WebSocketClientStrategy(uri)},
            {"udp", () => UdpClientStrategy.Of(uri)},
        };
        string scheme = uri.Scheme;
        if (strategies.ContainsKey(scheme)) return strategies[scheme]();
        throw new UriFormatException();
    }

    private void deactivateStateParams()
    {
        _addressInput.text = string.Empty;
        _starting = false;
    }
}
