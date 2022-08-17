using UnityEngine;
using Zenject;

public class HangerInstaller : MonoInstaller
{
    [SerializeField] private GameObject _ropeLoopObject;

    [SerializeField] private HangerPartParams[] _parts;

    public override void InstallBindings()
    {
        Container.Bind<RopeLoopHolder>()
            .AsSingle()
            .WithArguments(_ropeLoopObject);

        for (int i = 0; i < _parts.Length; ++i)
            Container.BindInterfacesAndSelfTo<HangerPart>()
                .AsCached()
                .WithArguments(_parts[i], i);
    }
}
