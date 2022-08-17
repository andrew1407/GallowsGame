using System.Collections.Generic;
using UnityEngine;

public enum AnimationState
{
    IDLE,
    STANDING,
    HANGED,
    WALKING,
    TALKING,
    WIN_REACTION,
}

public class AnimationStateController
{
    private readonly Dictionary<AnimationState, int> _hashes = new() {
        {AnimationState.IDLE, Animator.StringToHash("Idle")},
        {AnimationState.WALKING, Animator.StringToHash("Walking")},
        {AnimationState.STANDING, Animator.StringToHash("Standing")},
        {AnimationState.TALKING, Animator.StringToHash("Talking")},
        {AnimationState.HANGED, Animator.StringToHash("Hanged")},
        {AnimationState.WIN_REACTION, Animator.StringToHash("Win")},
    };

    private readonly Animator _animator;

    private AnimationState _currentState;

    public AnimationState CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            _animator.Play(_hashes[_currentState]);
        }
    }

    public AnimationStateController(Animator animator) => _animator = animator;
}
