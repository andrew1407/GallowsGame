using UnityEngine;

public class Runner : MonoBehaviour, IResetable
{
    [SerializeField] private Vector3 _startPosition;

    [SerializeField] private float _speed;

    [SerializeField] private Animator _animator;

    private const int OUT_OF_BORDER = 14;

    private readonly int _idleHash = Animator.StringToHash("Idle");

    private readonly int _runHash = Animator.StringToHash("Run");

    public bool Running { get; private set; }

    public void ResetState()
    {
        transform.position = _startPosition;
        _animator.Play(_idleHash);
        Running = false;
    }

    public void StartRun()
    {
        Running = true;
        _animator.Play(_runHash);
    }

    void Update()
    {
        if (!Running) return;
        transform.position += new Vector3(Time.deltaTime * _speed, 0, 0);
        if (transform.position.x < OUT_OF_BORDER) return;
        Running = false;
        _animator.Play(_idleHash);
    }
}
