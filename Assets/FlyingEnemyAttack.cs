using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class FlyingEnemyAttack : AttackState
{
    [SerializeField]
    private ClipTransition attackAnim;

    [SerializeField]
    private float attackForwardForce;

    private EnemyMovementControllerFlying movementController;

    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    [SerializeField]
    private float drag = 8;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);

        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(true);

        AnimancerState currentState = _ActionManager.anim.Play(attackAnim);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    protected override void OnDisable()
    {
        movementController.SetAllowRotation(true);
    }

    private void Update()
    {
        movementController.SetPathfindingDestination(Player.instance.transform.position);
    }

    public void AttackStart()
    {
        movementController.SetGroundDrag(drag);
        movementController.SetAllowMovement(false);
        _Character.SetIsKnockbackImmune(true);
        movementController.ApplyImpulseForce(_Character.transform.forward, attackForwardForce);
    }

    public void AttackEnd()
    {
        _Character.SetIsKnockbackImmune(false);
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Hitstun, true);
        movementController.ResetGroundDrag();
    }
}
