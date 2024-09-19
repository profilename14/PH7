using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public abstract class CharacterActionManager : MonoBehaviour
{
    [Header("Base Action Manager Data")]
    [SerializeField]
    private AnimancerComponent _Anim;
    public AnimancerComponent anim => _Anim;

    [SerializeField]
    private CharacterState _Idle;

    public readonly StateMachine<CharacterState>.WithDefault StateMachine = new();

    // A dictionary of which states are allowed to be entered, referenced by the states.
    // May want to refactor this so multiple similar states can be grouped together.
    protected Dictionary<CharacterState, bool> _AllowedActions;
    public Dictionary<CharacterState, bool> allowedActions => _AllowedActions;

    protected virtual void Awake()
    {
        StateMachine.DefaultState = _Idle;
    }

    public virtual void SetActionAllowed(CharacterState state)
    {
        _AllowedActions[state] = true;
    }

    public virtual void SetActionNotAllowed(CharacterState state)
    {
        _AllowedActions[state] = false;
    }

    public virtual void SetAllActionsAllowed(bool b)
    {
        foreach(var key in new List<CharacterState>(_AllowedActions.Keys))
        {
            _AllowedActions[key] = b;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref _Anim);
    }
#endif
}
