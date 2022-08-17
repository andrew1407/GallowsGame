using System.Collections;
using Zenject;

public class DifficultyViewController : IViewController, IResetable
{
    [Inject] private readonly ViewTools _viewTools;

    [Inject] private readonly string _text;

    private bool _oncePerformed = false;

    public IEnumerator PlayActions(GameProgress _)
    {
        if (_oncePerformed) yield break;
        _viewTools.TextTabsContainer.Message = _text;
        _oncePerformed = true;
    }

    public void ResetState() => _oncePerformed = false;
}