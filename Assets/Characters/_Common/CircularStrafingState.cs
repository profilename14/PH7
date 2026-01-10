using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class CircularStrafingState : CharacterState
{
    [SerializeField]
    private ClipTransition followAnimation;

    private EnemyMovementController movementController;
    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    [SerializeField] float maxFollowTime;
    [SerializeField] float followSpeed;
    [SerializeField] float rotationSpeed;

    [SerializeField] bool isStrafingRight;

    [SerializeField] float playerFollowRate;

    //[SerializeField] float minCirclingDistance;
    //[SerializeField] float maxCirclingDistance;
    [SerializeField] float desiredCirclingDistance;
    private float currentCirclingDistance;
    private float startCirclingDistance;
    [SerializeField] float circlingSpeed;

    [SerializeField] AttackCombo combo;
    [SerializeField] float enableComboAtPercent;

    [SerializeField]
    bool enableOverwritingState;

    private float moveTimer;

    private Vector3 startPlayerPosition;
    private Vector3 startForward;
    private float startAngle;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if(!enableOverwritingState) _ActionManager.SetAllActionPriorityAllowed(false);

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(false);
        movementController.SetForceManualRotation(false);

        AnimancerState currentState = _ActionManager.anim.Play(followAnimation);

        Vector3 pos1 = Player.instance.transform.position;
        pos1.y = 0;
        Vector3 pos2 = character.transform.position;
        pos2.y = 0;

        moveTimer = 0;

        movementController.pathfinding.maxSpeed = followSpeed;
        startCirclingDistance = Vector3.Distance(character.transform.position, Player.instance.transform.position);
        startPlayerPosition = Player.instance.transform.position;
        startForward = character.transform.forward;
        startAngle = character.transform.rotation.y;
        movementController.SetPathfindingDestination(startPlayerPosition);
    }

    protected override void OnDisable()
    {
        movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
        movementController.pathfinding.rotationSpeed = character.characterData.rotationSpeed;
        movementController.SetAllowRotation(true);
        if(combo != null) combo.SetAllowFollowup(false);
    }

    private void Update()
    {
        moveTimer += Time.deltaTime;
        float progress = moveTimer / maxFollowTime;

        if (progress > enableComboAtPercent && combo != null) combo.SetAllowFollowup(true);

        currentCirclingDistance = Mathf.Lerp(startCirclingDistance, desiredCirclingDistance, progress);

        if (progress > 1)
        {
            _ActionManager.StateMachine.ForceSetDefaultState();
        }

        Vector3 followPlayerVector = (Player.instance.transform.position - movementController.pathfinding.destination).normalized * playerFollowRate;

        movementController.SetPathfindingDestination(startPlayerPosition + followPlayerVector * Time.deltaTime);

        var normal = (character.transform.position - movementController.pathfinding.destination).normalized;
        var tangent = Vector3.Cross(normal, Player.instance.transform.up);

        // We can accomplish circling by getting the tangent of the vector to the player and offsetting it (for speed).
        if (isStrafingRight)
        {
            Vector3 destination = movementController.pathfinding.destination + normal * currentCirclingDistance + tangent * circlingSpeed;
            movementController.SetPathfindingDestination(destination);
        }
        else
        {
            movementController.SetPathfindingDestination(movementController.pathfinding.destination + normal * currentCirclingDistance + tangent * -circlingSpeed);
        }

        character.gameObject.transform.rotation = movementController.pathfinding.SimulateRotationTowards((Player.instance.transform.position - character.transform.position), rotationSpeed * Time.deltaTime);
    }
}
