using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using Object = UnityEngine.Object;

public class CharacterBodyViewTests
{
    private readonly List<GameObject> _objects = new();

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in _objects) Object.DestroyImmediate(obj);
        _objects.Clear();
    }

    [Test]
    public void Should_Toggle_Body_Renderer()
    {
        CharacterBodyView bodyView = makeTestable(out SpriteRenderer body, out _);

        bodyView.BodyVisible = false;
        Assert.False(body.enabled);
        Assert.False(bodyView.BodyVisible);

        bodyView.BodyVisible = true;
        Assert.True(body.enabled);
        Assert.True(bodyView.BodyVisible);
    }

    [Test]
    public void Should_Hide_All_Body_Parts()
    {
        CharacterBodyView bodyView = makeTestable(out SpriteRenderer body, out var parts);

        bodyView.HideAllBodyParts();

        foreach (var part in parts)
            Assert.False(part.Value.enabled, message: $"{part.Key} should be hidden");
        Assert.True(body.enabled, message: "Body itself isn't a body part");
    }

    [Test]
    public void Should_Show_Only_Selected_Body_Part()
    {
        CharacterBodyView bodyView = makeTestable(out _, out var parts);

        bodyView.HideAllBodyParts();
        bodyView.VisiblePart = BodyPart.TORSO;

        foreach (var part in parts)
            Assert.AreEqual(part.Key == BodyPart.TORSO, part.Value.enabled, message: $"Invalid visibility for {part.Key}");
    }

    private CharacterBodyView makeTestable(out SpriteRenderer body, out Dictionary<BodyPart, SpriteRenderer> parts)
    {
        body = makeRenderer("Body");
        parts = new();
        foreach (BodyPart part in Enum.GetValues(typeof(BodyPart)))
            parts[part] = makeRenderer(part.ToString());
        return new CharacterBodyView(body, parts);
    }

    private SpriteRenderer makeRenderer(string name)
    {
        var obj = new GameObject(name);
        _objects.Add(obj);
        return obj.AddComponent<SpriteRenderer>();
    }
}
