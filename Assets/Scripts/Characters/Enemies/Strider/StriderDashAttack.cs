using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class StriderDashAttack : AttackState
{
    [SerializeField]
    private ClipTransition dashAttack;

    [SerializeField]
    private float dashForce;

    [SerializeField]
    private float dashDrag;

    private EnemyMovementController movementController;
    private StriderVFXManager vfx;

    private float defaultDrag;

    public override bool CanEnterState => _ActionManager.allowedStates[this];

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        vfx = (StriderVFXManager)_Character.VFXManager;
        defaultDrag = movementController.rb.drag;
    }

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);
        _Character.SetIsKnockbackImmune(false);

        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(false);
        movementController.SetForceManualRotation(false);

        AnimancerState currentState = _ActionManager.anim.Play(dashAttack);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    private void Update()
    {
        movementController.SetPathfindingDestination(Player.instance.transform.position);
    }

    public void DashStart()
    {
        _ActionManager.SetAllActionPriorityAllowed(false);
        movementController.SetAllowRotation(false);
        _Character.SetIsKnockbackImmune(true);
        movementController.SetGroundDrag(dashDrag);
        movementController.ApplyImpulseForce(_Character.transform.forward, dashForce);
        vfx.SetDashTrailEmission(true);
        vfx.SetIsDashGlowing(true);
    }

    public void DashEnd()
    {
        _Character.SetIsKnockbackImmune(false);
        movementController.SetGroundDrag(defaultDrag);
        vfx.SetIsDashGlowing(false);
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Hitstun, true);
    }

    public void StopDashTrail()
    {
        vfx.SetDashTrailEmission(false);
    }
}
