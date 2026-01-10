using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class HalberdGolemOverheadSwing : AttackState
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

    [SerializeField]
    private GameObject saltShockwave;

    [SerializeField]
    private Transform saltShockwaveSpawnpoint;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        vfx = (SlashAttackVFXManager)_Character.VFXManager;
    }

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowed(false);

        base.OnEnable();

        //Debug.Log("Entering overhead swing");

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(true);
        movementController.SetForceLookAtPlayer(true);

        AnimancerState currentState = _ActionManager.anim.Play(swingAttack);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    protected override void OnDisable()
    {
        _ActionManager.SetAllActionPriorityAllowed(true);

        base.OnDisable();
        //Debug.Log("Exiting overhead swing");
    }

    private void Update()
    {
        Vector3 dirToPlayer = (Player.instance.transform.position - _Character.transform.position).normalized;
        movementController.SetPathfindingDestination(Player.instance.transform.position - (dirToPlayer * preferredRange));
    }

    public void SwingStart()
    {
        movementController.SetGroundDrag(drag);
        _Character.SetIsKnockbackImmune(true);
        movementController.ApplyImpulseForce(_Character.transform.forward, swingForwardForce);
        vfx.PlaySlashVFX();
    }

    public void StopTracking()
    {
        movementController.SetAllowRotation(false);
        movementController.SetAllowMovement(false);
    }

    public void SpawnSaltShockwave()
    {
        Instantiate(saltShockwave, saltShockwaveSpawnpoint.position, saltShockwaveSpawnpoint.rotation);
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

