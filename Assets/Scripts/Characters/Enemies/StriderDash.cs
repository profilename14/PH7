using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class StriderDash : AttackState
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
        movementController = (EnemyMovementController)_Character.movementController;
        vfx = (StriderVFXManager)_Character.VFXManager;
        defaultDrag = movementController.rb.drag;
    }

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowed(false);

        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(true);

        AnimancerState currentState = _ActionManager.anim.Play(dashAttack);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    private void Update()
    {
        movementController.SetPathfindingDestination(Player.instance.transform.position);
    }

    public void DashStart()
    {
        movementController.SetAllowRotation(false);
        _Character.SetIsKnockbackImmune(true);
        movementController.SetDrag(dashDrag);
        movementController.ApplyImpulseForce(_Character.transform.forward, dashForce);
        vfx.SetDashTrailEmission(true);
        vfx.SetIsDashGlowing(true);
    }

    public void DashEnd()
    {
        _Character.SetIsKnockbackImmune(false);
        movementController.SetDrag(defaultDrag);
        vfx.SetIsDashGlowing(false);
    }

    public void StopDashTrail()
    {
        vfx.SetDashTrailEmission(false);
    }
}
