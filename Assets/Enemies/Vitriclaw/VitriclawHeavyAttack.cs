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

    private EnemyMovementController movementController;
    private SlashAttackVFXManager vfx;
    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    [SerializeField]
    private float drag = 8;

    [SerializeField]
    private float windupMoveSpeed;

    [SerializeField]
    private float windupRotationSpeed;

    [SerializeField]
    private float playerTrackingDistance;

    [SerializeField]
    private bool cancelDefaultTracking = false;

    private bool isTracking = false;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        //vfx = (SlashAttackVFXManager)_Character.VFXManager;
    }

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);


        base.OnEnable();

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);

        if(!cancelDefaultTracking) SetTrackingAtDistance(playerTrackingDistance);

        AnimancerState currentState = _ActionManager.anim.Play(clawAttack);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

        movementController.pathfinding.maxSpeed = windupMoveSpeed;
        movementController.pathfinding.rotationSpeed = windupRotationSpeed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
        movementController.pathfinding.rotationSpeed = character.characterData.rotationSpeed;
        movementController.SetAllowRotation(true);
    }

    private void Update()
    {
        if (isTracking) SetTrackingAtDistance(playerTrackingDistance);
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

    public void SetTrackingAtDistance(float targetDistance)
    {
        isTracking = true;
        movementController.SetForceLookAtPlayer(true);
        movementController.SetForceManualRotation(true);
        movementController.SetPathfindingDestination((Player.instance.transform.position) + 
            (character.transform.position - Player.instance.transform.position).normalized * targetDistance);
    }

    public void StopTracking()
    {
        isTracking = false;
        movementController.SetAllowRotation(false);
    }
}
