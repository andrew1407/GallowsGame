using System.Collections.Generic;
using UnityEngine;

public enum BodyPart
{
    HEAD,
    TORSO,
    LEFT_HAND,
    LEFT_LEG,
    RIGHT_HAND,
    RIGHT_LEG,
}

public class CharacterBodyView
{
    private readonly SpriteRenderer _bodyRenderer;

    private readonly Dictionary<BodyPart, SpriteRenderer> _partsRenderers;

    public bool BodyVisible
    {
        get => _bodyRenderer.enabled;
        set => _bodyRenderer.enabled = value;
    }

    public BodyPart VisiblePart
    {
        set => _partsRenderers[value].enabled = true;
    }

    public CharacterBodyView(SpriteRenderer bodyRenderer, Dictionary<BodyPart, SpriteRenderer> partsRenderers)
    {
        _bodyRenderer = bodyRenderer;
        _partsRenderers = partsRenderers;
    }

    public void HideAllBodyParts()
    {
        foreach (var part in _partsRenderers.Values) part.enabled = false;
    }
}
