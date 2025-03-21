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

    [SerializeField]
    private float playerTargetDistance;

    private EnemyMovementController movementController;
    private ScuttlerVFXManager vfx;
    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

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
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(true);
        movementController.SetForceLookAtPlayer(true);

        AnimancerState currentState = _ActionManager.anim.Play(clawAttack);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    private void Update()
    {
        movementController.SetPathfindingDestination((Player.instance.transform.position) + (character.transform.position - Player.instance.transform.position).normalized * playerTargetDistance);
    }

    public void ClawStart()
    {
        movementController.SetGroundDrag(drag);
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
        movementController.ResetGroundDrag();
    }
}
