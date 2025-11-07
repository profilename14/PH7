using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class WarStriderSpitAttack : CharacterState
{
    [SerializeField]
    private ClipTransition spitAnimation;

    [SerializeField]
    private Transform projectileSpawnLocation;

    [SerializeField]
    GameObject projectilePrefab;

    [SerializeField]
    AttackData projectileStats;

    private EnemyMovementController movementController;

    private EnemyMovementControllerFlying flyingMovementController;

    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    [SerializeField]
    bool isFlying;

    private void Awake()
    {
        base.Awake();
        if (!isFlying) gameObject.GetComponentInParentOrChildren(ref movementController);
        else gameObject.GetComponentInParentOrChildren(ref flyingMovementController);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _ActionManager.SetAllActionPriorityAllowed(false);
        _Character.SetIsKnockbackImmune(false);

        if (!isFlying)
        {
            movementController.SetAllowMovement(false);
            movementController.SetAllowRotation(true);
            movementController.SetForceManualRotation(true);
        }
        else
        {
            flyingMovementController.SetAllowMovement(false);
            flyingMovementController.SetAllowRotation(true);
        }

        AnimancerState currentState = _ActionManager.anim.Play(spitAnimation);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    public void CreateProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnLocation.position, Quaternion.identity);
        MyProjectile projectileScript = projectile.GetComponent<MyProjectile>();

        Vector3 shotDirection = (Player.instance.transform.position - projectileSpawnLocation.position + Vector3.up).normalized;

        if(projectileScript != null)
        {
            projectileScript.InitProjectile(projectileSpawnLocation.position, Quaternion.LookRotation(shotDirection, Vector3.up), character, projectileStats);
        }
    }

    protected void Update()
    {
        if (!isFlying) movementController.SetPathfindingDestination(Player.instance.transform.position);
        else flyingMovementController.SetPathfindingDestination(Player.instance.transform.position);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
