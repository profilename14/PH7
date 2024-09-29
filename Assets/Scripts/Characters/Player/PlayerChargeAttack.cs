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
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Low];

    protected override void OnEnable()
    {
        attackCharged = false;

        _ActionManager.SetAllActionPriorityAllowed(false);

        rotationController.snapToCurrentMouseAngle();

        _ActionManager.anim.Play(chargingAnimation);
    }

    public void ReleaseChargeAttack()
    {
        if (attackCharged)
        {
            _ActionManager.SetAllActionPriorityAllowed(false);
            // Do a charge attack, go back to idle at the end.
            _ActionManager.anim.Play(chargeAttackAnimation).Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
        else
        {
            _ActionManager.StateMachine.ForceSetDefaultState();
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
