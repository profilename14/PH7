using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class TargetFollowMoveState : CharacterState
{
    [SerializeField]
    private ClipTransition followAnimation;

    private EnemyMovementController movementController;
    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    [SerializeField]
    private float jumpDuration;

    private bool updateEndpoint;

    private Vector3 startPoint;

    [SerializeField] float maxFollowTime;
    [SerializeField] float followDistance;
    [SerializeField] float followSpeed;

    [SerializeField] bool playerIsTarget;
    [SerializeField] Transform followingTarget;
    [SerializeField] bool rotateTowardsTarget;

    // Offset degrees from the forward vector that the enemy
    // will orient towards when moving towards target with
    // rotateTowardsTarget enabled.
    [SerializeField] float followForwardDirectionOffset;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _ActionManager.SetAllActionPriorityAllowed(false);

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(true);

        AnimancerState currentState = _ActionManager.anim.Play(followAnimation);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

        startPoint = character.transform.position;

        movementController.pathfinding.maxSpeed = followSpeed;

        if (playerIsTarget) followingTarget = Player.instance.transform;
    }

    protected override void OnDisable()
    {
        movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
        movementController.pathfinding.rotationSpeed = character.characterData.rotationSpeed;
        movementController.SetAllowRotation(true);
    }

    private void Update()
    {
            //movementController.SetPathfindingDestination(playerPosition);

            /*if (distanceToPlayer < closeFollowDistance)
            {
                movementController.SetAllowMovement(false);
            }
            else
            {
                movementController.SetAllowMovement(true);
            }*/
            /*case VitriclawMovementState.MidRangeCircling:
                if (!isCircling)
                {
                    Debug.Log("Circling");
                    isCircling = true;
                    movementController.pathfinding.maxSpeed = circlingSpeed;
                    movementController.SetAllowRotation(true);
                    movementController.SetForceLookAtPlayer(true);
                    movementController.SetForceManualRotation(true);
                    isCirclingRight = Random.Range(0f, 1f) > .5f;
                    circlingTimer = 0;
                }

                movementController.SetPathfindingDestination(Player.instance.transform.position);

                var normal = (character.transform.position - movementController.pathfinding.destination).normalized;
                var tangent = Vector3.Cross(normal, Player.instance.transform.up);

                // We can accomplish circling by getting the tangent of the vector to the player and offsetting it (for speed).
                if (isCirclingRight)
                {
                    Vector3 destination = movementController.pathfinding.destination + normal * midCirclingDistance + tangent * 4.5f;
                    movementController.SetPathfindingDestination(destination);
                }
                else
                {
                    movementController.SetPathfindingDestination(movementController.pathfinding.destination + normal * midCirclingDistance + tangent * -4.5f);
                }
                //if (!movementController.pathfinding.pathPending) movementController.pathfinding.SearchPath();

                circlingTimer += Time.deltaTime;
                if (circlingTimer > circlingTime)
                {
                    Debug.Log("Stop circling");
                    isCircling = false;
                    movementState = VitriclawMovementState.CloseRangeFollow;
                }

                if (updateEndpoint) endpoint = new Vector3(Player.instance.transform.position.x, groundY, Player.instance.transform.position.z);
            */
    }
}
