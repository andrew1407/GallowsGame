using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainCharacterInstaller : MonoInstaller
{
    [Header("Components")]

    [SerializeField] private Transform _transform;

    [SerializeField] private Animator _animator;

    [SerializeField] private SpriteRenderer _bodyRenderer;

    [Header("Movement")]

    [SerializeField] private float _walkSpeed;

    [SerializeField] private float _jumpSpeed;

    [Header("Body Parts")]

    [SerializeField] private SpriteRenderer _head;

    [SerializeField] private SpriteRenderer _torso;

    [SerializeField] private SpriteRenderer _leftHand;

    [SerializeField] private SpriteRenderer _leftLeg;

    [SerializeField] private SpriteRenderer _rightHand;

    [SerializeField] private SpriteRenderer _rightLeg;

    public override void InstallBindings()
    {
        Dictionary<BodyPart, SpriteRenderer> partsRenderers = new() {
            {BodyPart.HEAD, _head},
            {BodyPart.TORSO, _torso},
            {BodyPart.LEFT_HAND, _leftHand},
            {BodyPart.LEFT_LEG, _leftLeg},
            {BodyPart.RIGHT_HAND, _rightHand},
            {BodyPart.RIGHT_LEG, _rightLeg},
        };

        Container.Bind<AnimationStateController>()
            .AsSingle()
            .WithArguments(_animator);

        Container.Bind<CharacterBodyView>()
            .AsSingle()
            .WithArguments(_bodyRenderer, partsRenderers);
        
        Container.Bind<MovementController>()
            .AsSingle()
            .WithArguments(_transform, _walkSpeed, _jumpSpeed);
    }
}
