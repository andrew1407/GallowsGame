using System;
using UnityEngine;
using Zenject;

public class ViewControllersInstaller : MonoInstaller
{
    [SerializeField] private TextTabsContainer.Tabs _tabs;

    [SerializeField] private CharacterMovement _characterMovement;

    [SerializeField] private ComponentsSpawner _componentsSpawner;

    private readonly ResourceLoader _resourceLoader = new();

    public override void InstallBindings()
    {
        var tabsContainer = new TextTabsContainer(_tabs);
        var viewTools = new ViewTools() {
            CharacterMovement = _characterMovement,
            ComponentsSpawner = _componentsSpawner,
            TextTabsContainer = tabsContainer,
        };

        Container.BindInstance(viewTools);

        Container.BindInterfacesAndSelfTo<PrologueViewController>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameplayViewController>().AsSingle();
        Container.BindInterfacesAndSelfTo<WinViewController>().AsSingle();
        bindDifficultyWithInstaller<DifficultyViewController>(difficultyInstaller);
        bindDifficultyWithInstaller<LossViewController>(lossInstaller);
    }

    private void bindDifficultyWithInstaller<T>(Action<DiContainer> installer)
    {
        Container.BindInterfacesAndSelfTo<T>()
            .FromSubContainerResolve()
            .ByMethod(installer)
            .AsSingle();
    }

    private void difficultyInstaller(DiContainer container)
    {
        string difficlutiesDescription = _resourceLoader.LoadDifficlutiesDescription();
        container.BindInstance(difficlutiesDescription);
        container.Bind<DifficultyViewController>().AsSingle();
    }

    private void lossInstaller(DiContainer container)
    {
        container.Bind<LossViewController>().AsSingle();
        string message = _resourceLoader.LoadLossMessage();
        container.BindInstance(message);
    }
}
