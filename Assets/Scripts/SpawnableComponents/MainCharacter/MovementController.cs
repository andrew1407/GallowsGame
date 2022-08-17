using UnityEngine;

public enum MovementState
{
    IDLE,
    JUMP,
    WALK,
}

public class MovementController
{
    private readonly float _walkSpeed;

    private readonly float _jumpSpeed;

    private readonly Transform _transform;

    private Vector3 destination;

    public MovementState State { get; private set; }

    public MovementController(Transform transform, float walkSpeed, float jumpSpeed)
    {
        _transform = transform;
        _walkSpeed = walkSpeed;
        _jumpSpeed = jumpSpeed;
        State = MovementState.IDLE;
    }

    public void Action()
    {
        if (State == MovementState.IDLE) return;
        float speed = State == MovementState.JUMP ? _jumpSpeed : _walkSpeed;
        Vector3 previous = _transform.position;
        _transform.position = Vector3.MoveTowards(previous, destination, speed * Time.deltaTime);
        if (previous == _transform.position) Stop();
    }

    public void Go(Vector3 whereTo) => setState(MovementState.WALK, whereTo);

    public void Jump(Vector3 heightPoint) => setState(MovementState.JUMP, heightPoint);

    public void Stop() => setState(MovementState.IDLE, Vector3.zero);

    private void setState(MovementState state, Vector3 point)
    {
        State = state;
        destination = point;
    }
}
