using System;
using UnityEngine;
using Zenject;

[Serializable]
public struct HangerPartParams
{
    public Transform Transform;

    public float Speed;

    public Vector3 StartPosition;

    public Vector3 FinishPosition;

    public AnimationCurve Curve;
}

public class HangerPart : ITickable, IInitializable, IResetable
{
    public readonly int Index;

    private readonly HangerPartParams _partParams;

    private readonly Vector3 _direction;

    private float _progress;

    public bool Moving { get; private set; }

    public HangerPart(HangerPartParams partParams, int index)
    {
        _partParams = partParams;
        Index = index;
        _direction = _partParams.FinishPosition - _partParams.StartPosition;
    }

    public void StartMove() => Moving = true;

    public void ResetState()
    {
        Moving = false;
        _progress = 0;
        _partParams.Transform.localPosition = _partParams.StartPosition;
    }

    public void Initialize()
    {
        _partParams.Transform.localPosition = _partParams.StartPosition;
    }

    public void Tick()
    {
        if (!Moving) return;
        _progress += Time.deltaTime;
        Vector3 previous = _partParams.Transform.localPosition;
        _partParams.Transform.localPosition = Vector3.Lerp(_partParams.StartPosition, _partParams.FinishPosition, _partParams.Curve.Evaluate(_progress));
        if (previous == _partParams.Transform.localPosition) Moving = false;
    }
}
