using System;
using System.Collections.Generic;
using Zenject;

public class ViewsContainer : IResetable
{
    private readonly Dictionary<string, IViewController> _viewControllers = new();

    [Inject]
    private void Construct(IViewController[] viewControllers)
    {
        Dictionary<string, Type> types = new() {
            {GameStageLabels.Difficulty, typeof(DifficultyViewController)},
            {GameStageLabels.Prologue, typeof(PrologueViewController)},
            {GameStageLabels.Gameplay, typeof(GameplayViewController)},
            {GameStageLabels.Loss, typeof(LossViewController)},
            {GameStageLabels.Win, typeof(WinViewController)},
        };

        foreach (var view in types)
            _viewControllers[view.Key] = Array.Find(viewControllers, v => v.GetType() == view.Value);
    }

    public IViewController this[string stage] => _viewControllers[stage];

    public void ResetState()
    {
        foreach (var controller in _viewControllers.Values)
            if (controller is IResetable resetable) resetable.ResetState();
    }
}