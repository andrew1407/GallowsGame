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
#if UNITY_WEBGL && !UNITY_EDITOR
        // A page can't close its browser tab, so there's nothing for the exit button to do
        _exitButton.gameObject.SetActive(false);
#else
        _exitButton.onClick.AddListener(Application.Quit);
#endif
        _submitButton.onClick.AddListener(async () => await _gameLoop.PlayStage());
        _restartButton.onClick.AddListener(_gameLoop.ResetState);
        _gameModeSelector.Initialize();
        _gameLoop.Initialize();
    }

    private void OnDisable() => _gameLoop.Dispose();
}
