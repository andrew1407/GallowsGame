using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Hanger : MonoBehaviour, IResetable
{
    [Inject] private readonly RopeLoopHolder _ropeLoopHolder;

    public IReadOnlyList<HangerPart> Parts { get; private set; }

    [Inject]
    private void Construct(HangerPart[] hangerParts)
    {
        HangerPart[] partsOrdered = new HangerPart[hangerParts.Length];
        foreach (var part in hangerParts) partsOrdered[part.Index] = part;
        Parts = partsOrdered;
    }

    public void ResetState()
    {
        _ropeLoopHolder.ResetState();
        foreach (var part in Parts) part.ResetState();
    }
}
