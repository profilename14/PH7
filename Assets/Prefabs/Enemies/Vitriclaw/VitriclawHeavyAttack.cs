using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class VitriclawHeavyAttack : AttackState
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

    [SerializeField]
    private float windupMoveSpeed;

    [SerializeField]
    private float windupRotationSpeed;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        vfx = (ScuttlerVFXManager)_Character.VFXManager;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(true);
        movementController.SetForceLookAtPlayer(true);

        AnimancerState currentState = _ActionManager.anim.Play(clawAttack);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

        movementController.pathfinding.maxSpeed = windupMoveSpeed;
        movementController.pathfinding.rotationSpeed = windupRotationSpeed;
    }

    protected override void OnDisable()
    {
        movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
        movementController.pathfinding.rotationSpeed = character.characterData.rotationSpeed;
        movementController.SetAllowRotation(true);
    }

    private void Update()
    {
        movementController.SetPathfindingDestination((Player.instance.transform.position) + (character.transform.position - Player.instance.transform.position).normalized * playerTargetDistance);
    }

    public void StopTracking()
    {
        movementController.SetAllowRotation(false);
    }

    public void ClawStart()
    {
        movementController.SetGroundDrag(drag);
        movementController.SetAllowMovement(false);
        _Character.SetIsKnockbackImmune(true);
        movementController.ApplyImpulseForce(_Character.transform.forward, clawForwardForce);
        //vfx.PlayClawVFX();
    }

    public void ClawEnd()
    {
        _Character.SetIsKnockbackImmune(false);
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Hitstun, true);
        movementController.ResetGroundDrag();
    }
}
