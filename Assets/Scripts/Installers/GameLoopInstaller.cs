using UnityEngine;
using TMPro;
using Zenject;

public class GameLoopInstaller : MonoInstaller
{
    [SerializeField] private MonoBehaviour _monoBehaviour;

    [SerializeField] private TMP_InputField _inputField;

    [SerializeField] private StartOptionsContainer _startOptionsContainer;

    public override void InstallBindings()
    {
        Container.BindInstance(_startOptionsContainer);

        Container.Bind<GameModeSelector>().AsSingle();
        Container.Bind<GameplayStrategyFactory>().WhenInjectedInto<GameModeSelector>();

        Container.Bind<GameLoop>()
            .FromSubContainerResolve()
            .ByMethod(installGameLoop)
            .AsSingle();
    }

    private void installGameLoop(DiContainer container)
    {
        container.Bind<GameLoop>().AsSingle();
        container.Bind<MonoBehaviour>().FromInstance(_monoBehaviour).AsSingle();
        container.Bind<StageActivity>().AsSingle();
        container.Bind<ViewsContainer>().AsSingle();
        container.BindInstance(_inputField);
    }
}
