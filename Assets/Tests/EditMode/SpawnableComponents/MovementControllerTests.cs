using NUnit.Framework;
using UnityEngine;

public class MovementControllerTests
{
    private GameObject _gameObject;

    [SetUp]
    public void SetUp() => _gameObject = new GameObject(nameof(MovementControllerTests));

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(_gameObject);

    [Test]
    public void Should_Be_Idle_By_Default()
    {
        MovementController movementController = new(_gameObject.transform, walkSpeed: 1, jumpSpeed: 2);

        Assert.AreEqual(MovementState.IDLE, movementController.State);
    }

    [Test]
    public void Should_Switch_States_On_Go_Jump_And_Stop()
    {
        MovementController movementController = new(_gameObject.transform, walkSpeed: 1, jumpSpeed: 2);

        movementController.Go(Vector3.right);
        Assert.AreEqual(MovementState.WALK, movementController.State);

        movementController.Jump(Vector3.up);
        Assert.AreEqual(MovementState.JUMP, movementController.State);

        movementController.Stop();
        Assert.AreEqual(MovementState.IDLE, movementController.State);
    }

    [Test]
    public void Should_Not_Move_While_Idle()
    {
        Vector3 expected = new(1, 2, 3);
        _gameObject.transform.position = expected;
        MovementController movementController = new(_gameObject.transform, walkSpeed: 100, jumpSpeed: 100);

        movementController.Action();

        Assert.AreEqual(expected, _gameObject.transform.position);
    }

    [Test]
    public void Should_Stop_When_Destination_Is_Reached()
    {
        Vector3 destination = new(1, 2, 3);
        _gameObject.transform.position = destination;
        MovementController movementController = new(_gameObject.transform, walkSpeed: 1, jumpSpeed: 1);

        movementController.Go(destination);
        movementController.Action();

        Assert.AreEqual(MovementState.IDLE, movementController.State, message: "Movement should stop once there's nowhere to move");
        Assert.AreEqual(destination, _gameObject.transform.position);
    }

    [Test]
    public void Should_Not_Overshoot_Destination()
    {
        Vector3 destination = new(0.001f, 0, 0);
        _gameObject.transform.position = Vector3.zero;
        MovementController movementController = new(_gameObject.transform, walkSpeed: float.MaxValue, jumpSpeed: float.MaxValue);

        movementController.Go(destination);
        for (int i = 0; i < 3; ++i) movementController.Action();

        Assert.LessOrEqual(_gameObject.transform.position.x, destination.x);
    }
}
