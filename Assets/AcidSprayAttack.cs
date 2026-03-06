using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class AcidSprayAttack : AttackState
{
    [SerializeField]
    private ClipTransition walkAnim;

    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private float rotationSpeed;

    [SerializeField]
    private float playerTrackingDistance;

    [SerializeField]
    private float duration;

    [SerializeField]
    private float startSprayDelay;

    [SerializeField]
    GameObject effectField;

    [SerializeField]
    ParticleSystem spray;

    [SerializeField]
    GameObject chargeSpray;

    private EnemyMovementController movementController;

    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    private float timer;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);

        base.OnEnable();

        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(false);
        movementController.SetForceManualRotation(false);
        movementController.SetForceLookAtPlayer(false);
        movementController.pathfinding.rotationSpeed = rotationSpeed;
        movementController.pathfinding.maxSpeed = moveSpeed;

        timer = 0;

        chargeSpray.SetActive(true);

        _ActionManager.anim.Play(walkAnim);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
        movementController.pathfinding.rotationSpeed = character.characterData.rotationSpeed;
        movementController.SetForceManualRotation(false);
        movementController.SetForceLookAtPlayer(false);
        chargeSpray.SetActive(false);
        effectField.SetActive(false);
        spray.Stop(true);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer > startSprayDelay)
        {
            SetTrackingAtDistance(playerTrackingDistance);
            movementController.SetAllowMovement(true);
            movementController.SetAllowRotation(true);
            effectField.SetActive(true);

            if(!spray.isPlaying) spray.Play(true);

            if(timer > duration)
            {
                _ActionManager.StateMachine.ForceSetDefaultState();
            }
        }
    }


    public void SetTrackingAtDistance(float targetDistance)
    {
        movementController.SetForceLookAtPlayer(true);
        movementController.SetForceManualRotation(true);
        movementController.SetPathfindingDestination((Player.instance.transform.position) +
            (character.transform.position - Player.instance.transform.position).normalized * targetDistance);
    }
}
