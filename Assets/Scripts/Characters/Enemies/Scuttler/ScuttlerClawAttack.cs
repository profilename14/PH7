using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class ScuttlerClawAttack : AttackState
{
    [SerializeField]
    private ClipTransition clawAttack;

    [SerializeField]
    private float clawForwardForce;

    private EnemyMovementController movementController;
    private ScuttlerVFXManager vfx;
    public override bool CanEnterState => _ActionManager.allowedStates[this];

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
        movementController.SetForceManualRotation(true);

        AnimancerState currentState = _ActionManager.anim.Play(clawAttack);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    private void Update()
    {
        movementController.SetPathfindingDestination(Player.instance.transform.position);
    }

    public void ClawStart()
    {
        movementController.SetAllowRotation(false);
        movementController.SetAllowMovement(false);
        _Character.SetIsKnockbackImmune(true);
        movementController.ApplyImpulseForce(_Character.transform.forward, clawForwardForce);
        vfx.PlayClawVFX();
    }

    public void ClawEnd()
    {
        _Character.SetIsKnockbackImmune(false);
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Hitstun, true);
    }
}
