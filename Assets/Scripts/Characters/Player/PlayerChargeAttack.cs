using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class PlayerChargeAttack : AttackState
{
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private RotationController rotationController;

    [SerializeReference]
    ClipTransition chargingAnimation;

    [SerializeReference]
    ClipTransition chargeAttackAnimation;

    [SerializeField]
    AudioClip chargingSFX;

    [SerializeField]
    AudioClip chargeSwingSFX;

    [SerializeField]
    bool attackCharged;

    // Uses allowedActions to control if entering this state is allowed.
    public override bool CanEnterState 
        => actionManager.allowedActions[this];

    protected override void OnEnable()
    {
        attackCharged = false;

        actionManager.SetAllActionsAllowed(false);

        rotationController.snapToCurrentMouseAngle();

        actionManager.anim.Play(chargingAnimation);
    }

    public void ReleaseChargeAttack()
    {
        if (attackCharged)
        {
            actionManager.SetAllActionsAllowed(false);
            // Do a charge attack, go back to idle at the end.
            actionManager.anim.Play(chargeAttackAnimation).Events(this).OnEnd ??= actionManager.StateMachine.ForceSetDefaultState;
        }
        else
        {
            actionManager.StateMachine.ForceSetDefaultState();
        }
    }

    public void ChargingDone()
    {
        attackCharged = true;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
    }
#endif
}
