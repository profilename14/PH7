using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class HalberdSwing : AttackState
{
    [SerializeField]
    private ClipTransition swingAttack;

    [SerializeField]
    private float swingForwardForce;

    private EnemyMovementController movementController;
    private ScuttlerVFXManager vfx;
    public override bool CanEnterState => _ActionManager.allowedStates[this];

    [SerializeField]
    private float drag = 8;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        vfx = (ScuttlerVFXManager)_Character.VFXManager;
    }

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowed(false);

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(false);

        AnimancerState currentState = _ActionManager.anim.Play(swingAttack);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    private void Update()
    {
        movementController.SetPathfindingDestination(Player.instance.transform.position);
    }

    public void SwingStart()
    {
        movementController.SetGroundDrag(drag);
        movementController.SetAllowRotation(false);
        movementController.SetAllowMovement(false);
        _Character.SetIsKnockbackImmune(true);
        movementController.ApplyImpulseForce(_Character.transform.forward, swingForwardForce);
        vfx.PlayClawVFX();
    }

    public void SwingEnd()
    {
        _Character.SetIsKnockbackImmune(false);
        movementController.ResetGroundDrag();
    }
}

