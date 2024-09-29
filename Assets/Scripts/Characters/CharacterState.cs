using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public abstract class CharacterState : StateBehaviour
{
    [SerializeField]
    protected Character _Character;
    public Character character => _Character;

    [SerializeField]
    protected CharacterActionManager _ActionManager;
    public CharacterActionManager actionManager => _ActionManager;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        gameObject.GetComponentInParentOrChildren(ref _Character);
        gameObject.GetComponentInParentOrChildren(ref _ActionManager);
    }
#endif

    protected virtual void OnEnable()
    {
        return;
    }

    protected virtual void OnDisable()
    {
        return;
    }
}
