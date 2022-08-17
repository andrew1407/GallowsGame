using UnityEngine;

public class RopeLoopHolder : IResetable
{
    private readonly SpriteRenderer _loopRenderer;

    private readonly Transform _loopTransform;

    public RopeLoopHolder(GameObject _ropeLoop)
    {
        _loopRenderer = _ropeLoop.GetComponent<SpriteRenderer>();
        _loopTransform = _ropeLoop.transform;
    }

    public void ResetState() => _loopRenderer.enabled = true;

    public void Hang(Transform target) => target.SetParent(_loopTransform);
}
