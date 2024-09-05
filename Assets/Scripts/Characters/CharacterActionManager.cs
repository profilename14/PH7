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
    
    protected virtual void Awake()
    {
        StateMachine.DefaultState = _Idle;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref _Anim);
    }
#endif
}
