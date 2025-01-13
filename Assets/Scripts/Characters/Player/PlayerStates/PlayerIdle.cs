using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer.FSM;
using Animancer;

public class PlayerIdle : CharacterState
{
    // Probably won't do anything significant except hold the CharacterState fields
    // and maybe do some VFX for the idle state.

    [SerializeField]
    TransitionAsset idleAnimation;

    private PlayerActionManager actionManager;
    private PlayerMovementController movementController;

    protected override void OnEnable()
    {
        _ActionManager.anim.Play(idleAnimation);
    }

    protected void Update()
    {
        movementController.ProcessMoveInput(actionManager.GetDirectionalInput().moveDir);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref actionManager);
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }
#endif
}
