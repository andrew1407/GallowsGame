using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Button _submitButton;

    [SerializeField] private Button _restartButton;

    [SerializeField] private Button _exitButton;

    [SerializeField] private Button _infoButton;

    [Inject] private readonly GameLoop _gameLoop;

    [Inject] private readonly GameModeSelector _gameModeSelector;

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // A page can't close its browser tab, so there's nothing for the exit button to do
        _exitButton.gameObject.SetActive(false);
        takeExitButtonPlace();
#else
        _exitButton.onClick.AddListener(Application.Quit);
#endif
        _submitButton.onClick.AddListener(async () => await _gameLoop.PlayStage());
        _restartButton.onClick.AddListener(_gameLoop.ResetState);
        _gameModeSelector.Initialize();
        _gameLoop.Initialize();
    }

    private void OnDisable() => _gameLoop.Dispose();

    // The info and restart buttons slide right into the exit button's corner (both are anchored there),
    // so the info button keeps as much room to the right edge as to the bottom one and the spacing between them stays
    private void takeExitButtonPlace()
    {
        var info = (RectTransform)_infoButton.transform;
        var restart = (RectTransform)_restartButton.transform;
        float bottomMargin = info.anchoredPosition.y - info.sizeDelta.y * info.pivot.y;
        float rightMargin = -(info.anchoredPosition.x + info.sizeDelta.x * (1 - info.pivot.x));
        Vector2 shift = new(rightMargin - bottomMargin, 0);
        info.anchoredPosition += shift;
        restart.anchoredPosition += shift;
    }
}
