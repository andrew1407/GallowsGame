using UnityEngine;
using Zenject;

public class RopeLoop : MonoBehaviour
{
    [Inject] private readonly RopeLoopHolder _ropeLoopHolder;

    public void Hang(Transform target) => _ropeLoopHolder.Hang(target);
}
