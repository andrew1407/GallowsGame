using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Button _submitButton;

    [SerializeField] private Button _restartButton;

    [SerializeField] private Button _exitButton;

    [Inject] private readonly GameLoop _gameLoop;

    [Inject] private readonly GameModeSelector _gameModeSelector;

    private void Start()
    {
        _exitButton.onClick.AddListener(Application.Quit);
        _submitButton.onClick.AddListener(async () => await _gameLoop.PlayStage());
        _restartButton.onClick.AddListener(_gameLoop.ResetState);
        _gameModeSelector.Initialize();
        _gameLoop.Initialize();
    }

    private void OnDisable() => _gameLoop.Dispose();
}
