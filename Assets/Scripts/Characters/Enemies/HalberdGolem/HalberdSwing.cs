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
    private SlashAttackVFXManager vfx;
    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    [SerializeField]
    private float drag = 8;

    [SerializeField]
    private float preferredRange = 10;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        vfx = (SlashAttackVFXManager)_Character.VFXManager;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _ActionManager.SetAllActionPriorityAllowed(false);

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(true);
        movementController.SetForceLookAtPlayer(true);

        AnimancerState currentState = _ActionManager.anim.Play(swingAttack);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    private void Update()
    {
        Vector3 dirToPlayer = (Player.instance.transform.position - _Character.transform.position).normalized;
        movementController.SetPathfindingDestination(Player.instance.transform.position - (dirToPlayer * preferredRange));
    }

    public void SwingStart()
    {
        vfx.PlaySlashVFX();
        movementController.SetGroundDrag(drag);
        movementController.SetAllowRotation(false);
        movementController.SetAllowMovement(false);
        _Character.SetIsKnockbackImmune(true);
        movementController.ApplyImpulseForce(_Character.transform.forward, swingForwardForce);
    }

    public void Recovery()
    {
        _Character.SetIsKnockbackImmune(false);
        movementController.SetAllowRotation(false);
        movementController.SetAllowMovement(false);
    }

    public void EndAttack()
    {
        movementController.SetAllowRotation(true);
        movementController.SetAllowMovement(true);
        movementController.SetForceManualRotation(false);
        movementController.SetForceLookAtPlayer(true);
    }
}

