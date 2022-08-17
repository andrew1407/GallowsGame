using System;
using System.Data;
using UnityEngine;
using Zenject;

public class ComponentsSpawner : MonoBehaviour, IResetable
{
    [SerializeField] private GameObject _mainCharacterPrefab;
    
    [SerializeField] private GameObject _hangerPrefab;

    [SerializeField] private GameObject _runnerPrefab;

    [Inject] private readonly DiContainer _diContainer;

    public MainCharacter MainCharacter { get; private set; }

    public Hanger Hanger { get; private set; }

    public Runner Runner { get; private set; }

    public void SpawnMainCharacter()
    {
        checkAlreadyCreated(MainCharacter, nameof(MainCharacter));
        MainCharacter = spawn<MainCharacter>(_mainCharacterPrefab);
    }

    public void SpawnHanger()
    {
        checkAlreadyCreated(Hanger, nameof(Hanger));
        Hanger = spawn<Hanger>(_hangerPrefab);
    }

    public void SpawnRunner()
    {
        checkAlreadyCreated(Runner, nameof(Runner));
        Runner = spawn<Runner>(_runnerPrefab, hasInjections: false);
    }

    public void ResetState()
    {
        IResetable[] containers = { Hanger, MainCharacter, Runner };
        foreach (var value in containers) value.ResetState();

    }

    private T spawn<T>(GameObject prefab, bool hasInjections = true)
    {
        Func<GameObject, GameObject> instantiator = hasInjections ? _diContainer.InstantiatePrefab : Instantiate;
        return instantiator(prefab).GetComponent<T>();
    }
    
    private void checkAlreadyCreated<T>(T value, string name)
    {
        if (value != null) throw new DataException(name + " value has already been created");
    }
}
