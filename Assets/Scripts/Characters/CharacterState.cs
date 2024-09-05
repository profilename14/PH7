using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public abstract class CharacterState : StateBehaviour
{
    [SerializeField]
    private Character character;

    [SerializeField]
    private CharacterActionManager actionManager;

    [SerializeField]
    private TransitionAsset _EnterAnim;
    public TransitionAsset enterAnim => _EnterAnim;

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
        actionManager.anim.Play(_EnterAnim);
    }
}
