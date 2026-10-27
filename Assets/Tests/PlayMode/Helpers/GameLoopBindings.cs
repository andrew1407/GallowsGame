using TMPro;
using UnityEngine;
using Zenject;

// GameLoop with its real view controllers, bound as GameLoopInstaller and ViewControllersInstaller do,
// minus the spawned characters (so only the stages that don't move them can be played)
public static class GameLoopBindings
{
    public const string DifficultyText = "difficulties";

    public const string LossText = "loss";

    public static void Install(DiContainer container, TextTabsContainer.Tabs tabs, TMP_InputField inputField)
    {
        var runner = new GameObject(nameof(CoroutineRunner)).AddComponent<CoroutineRunner>();
        container.BindInstance(UiFactory.MakeViewTools(tabs));
        container.Bind<MonoBehaviour>().FromInstance(runner).AsSingle();
        container.BindInstance(inputField);
        container.Bind<StageActivity>().AsSingle();
        container.Bind<ViewsContainer>().AsSingle();
        container.Bind<GameLoop>().AsSingle();

        bindWithText<DifficultyViewController>(container, DifficultyText);
        bindWithText<LossViewController>(container, LossText);
        container.Bind<IViewController>().To<PrologueViewController>().AsSingle();
        container.Bind<IViewController>().To<GameplayViewController>().AsSingle();
        container.Bind<IViewController>().To<WinViewController>().AsSingle();
    }

    // Both controllers take a string, so each gets its own in a subcontainer
    private static void bindWithText<T>(DiContainer container, string text) where T : IViewController
    {
        container.Bind<IViewController>().To<T>()
            .FromSubContainerResolve()
            .ByMethod(subContainer =>
            {
                subContainer.BindInstance(text);
                subContainer.Bind<T>().AsSingle();
            })
            .AsSingle();
    }
}
