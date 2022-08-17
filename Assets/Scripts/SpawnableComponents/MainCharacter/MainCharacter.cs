using System.Collections;
using UnityEngine;
using Zenject;

public class MainCharacter : MonoBehaviour, IResetable
{
    [SerializeField] private Vector3 _startPosition;

    [SerializeField] private Collider2D _collider;

    [Inject] private readonly AnimationStateController _animationStateController;

    [Inject] private readonly CharacterBodyView _bodyView;

    [Inject] private readonly MovementController _movementController;

    private const string _runnerTag = "Runner";

    private const string _ropeLoopTag = "RopeLoop";

    public bool BodyVisible
    {
        get => _bodyView.BodyVisible;
        set
        {
            _bodyView.HideAllBodyParts();
            _bodyView.BodyVisible = value;
        }
    }

    public AnimationState CurrentState
    {
        get => _animationStateController.CurrentState;
        set => _animationStateController.CurrentState = value;
    }

    public void SetBodyPartVisible(BodyPart bodyPart) => _bodyView.VisiblePart = bodyPart;

    public void ResetState()
    {
        transform.SetParent(null);
        _collider.enabled = true;
        _movementController.Stop();
        transform.position = _startPosition;
        BodyVisible = true;
        CurrentState = AnimationState.IDLE;
    }

    public IEnumerator Move(Vector3 point)
    {
        CurrentState = AnimationState.WALKING;
        _movementController.Go(point);
        yield return new WaitUntil(() => _movementController.State == MovementState.IDLE);
        CurrentState = AnimationState.STANDING;
    }

    public IEnumerator Jump(Vector3 point)
    {
        CurrentState = AnimationState.IDLE;
        _movementController.Jump(point);
        yield return new WaitUntil(() => _movementController.State == MovementState.IDLE);
        CurrentState = AnimationState.STANDING;
    }

    private void Start() => transform.position = _startPosition;

    private void Update() => _movementController.Action();

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag(_runnerTag))
        {
            var runner = collider.GetComponent<Runner>();
            if (runner != null && runner.Running) BodyVisible = false;
            return;
        }
        if (collider.CompareTag(_ropeLoopTag))
        {
            var ropeLoop = collider.GetComponent<RopeLoop>();
            if (ropeLoop != null)
            {
                _collider.enabled = false;
                ropeLoop.Hang(transform);
            }
            return;
        }
    }
}
