using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public abstract class CharacterState : StateBehaviour
{
    [SerializeField]
    protected Character character;

    [SerializeField]
    protected CharacterActionManager actionManager;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        gameObject.GetComponentInParentOrChildren(ref character);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
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
