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

    private EnemyMovementController movementController;

    public override bool CanEnterState => _ActionManager.allowedStates[this];

    private void Awake()
    {
        movementController = (EnemyMovementController)_Character.movementController;
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
        //Debug.Log("Dash start");
        movementController.SetAllowRotation(false);
        movementController.ApplyImpulseForce(_Character.transform.forward, dashForce);
    }
}
