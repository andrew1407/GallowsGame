using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class HangerTests
{
    private readonly List<GameObject> _objects = new();

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in _objects) Object.DestroyImmediate(obj);
        _objects.Clear();
    }

    [Test]
    public void Should_Place_Part_At_Start_On_Initialize()
    {
        HangerPartParams partParams = makeParams();
        HangerPart part = new(partParams, index: 3);

        part.Initialize();

        Assert.AreEqual(3, part.Index);
        Assert.AreEqual(partParams.StartPosition, partParams.Transform.localPosition);
        Assert.False(part.Moving, message: "Part shouldn't move by default");
    }

    [Test]
    public void Should_Not_Move_Part_Before_Start()
    {
        HangerPartParams partParams = makeParams();
        HangerPart part = new(partParams, index: 0);

        part.Initialize();
        part.Tick();

        Assert.AreEqual(partParams.StartPosition, partParams.Transform.localPosition);
    }

    [Test]
    public void Should_Finish_Move_When_Curve_Stops_Changing_Position()
    {
        // A curve stuck at 0 keeps the part at its start, so the very first tick has nothing to move
        HangerPartParams partParams = makeParams(AnimationCurve.Constant(0, 1, 0));
        HangerPart part = new(partParams, index: 0);

        part.Initialize();
        part.StartMove();
        Assert.True(part.Moving);

        part.Tick();
        Assert.False(part.Moving, message: "Part should stop once its position doesn't change");
    }

    [Test]
    public void Should_Reset_Part_To_Start()
    {
        HangerPartParams partParams = makeParams();
        HangerPart part = new(partParams, index: 0);

        part.StartMove();
        partParams.Transform.localPosition = partParams.FinishPosition;
        part.ResetState();

        Assert.False(part.Moving);
        Assert.AreEqual(partParams.StartPosition, partParams.Transform.localPosition);
    }

    [Test]
    public void Should_Hang_Target_On_Rope_Loop()
    {
        GameObject ropeLoop = makeObject("RopeLoop");
        ropeLoop.AddComponent<SpriteRenderer>();
        Transform target = makeObject("Target").transform;
        RopeLoopHolder holder = new(ropeLoop);

        holder.Hang(target);

        Assert.AreEqual(ropeLoop.transform, target.parent);
    }

    [Test]
    public void Should_Show_Rope_Loop_On_Reset()
    {
        GameObject ropeLoop = makeObject("RopeLoop");
        var renderer = ropeLoop.AddComponent<SpriteRenderer>();
        RopeLoopHolder holder = new(ropeLoop);

        renderer.enabled = false;
        holder.ResetState();

        Assert.True(renderer.enabled);
    }

    private HangerPartParams makeParams(AnimationCurve curve = null) => new() {
        Transform = makeObject("HangerPart").transform,
        Speed = 1,
        StartPosition = new Vector3(0, -5, 0),
        FinishPosition = Vector3.zero,
        Curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1),
    };

    private GameObject makeObject(string name)
    {
        var obj = new GameObject(name);
        _objects.Add(obj);
        return obj;
    }
}
