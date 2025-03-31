using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

enum VitriclawMovementState { CloseRangeFollow, MidRangeCircling, MidRangeStrafe, LongRangeStrafe};

public class VitriclawFollow : CharacterState
{
    [SerializeField]
    private ClipTransition idle;

    [SerializeField]
    private ClipTransition moveBody;

    [SerializeField]
    private float playerRangeSpan;

    private VitriclawMovementState movementState;

    [Header("CloseRangeFollow")]
    [SerializeField]
    private float closeFollowDistance;

    [Header("MidRangeCircling")]
    [SerializeField]
    private float midCirclingDistance;
    [SerializeField]
    private float circlingSpeed;
    [SerializeField]
    private float circlingTime;
    private bool isCircling;
    private bool isCirclingRight;
    private float circlingTimer;

    [Header("MidRangeStrafe")]
    [SerializeField]
    private float midStrafeDistance;
    [SerializeField]
    private float midStrafeAngle;
    [SerializeField]
    private float midStrafeSpeed;


    [Header("LongRangeStrafe")]
    [SerializeField]
    private float longStrafeDistance;
    [SerializeField]
    private float longStrafeSpeed;

    [SerializeField]
    private bool rotateToFaceMovementDirection = true;

    private EnemyMovementController movementController;

    private Vector3 playerPosition;

    private AnimancerLayer bodyLayer;

    private EnemyActionManager actionManager;

    private bool stopped;

    private void Awake()
    {
        base.Awake();
        actionManager = (EnemyActionManager) _ActionManager;
        bodyLayer = actionManager.anim.Layers[0];
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowed(true);
        bodyLayer.Play(moveBody);
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(!rotateToFaceMovementDirection);
    }

    private void Update()
    {
        playerPosition = Player.instance.transform.position;

        switch (movementState)
        {
            case VitriclawMovementState.CloseRangeFollow:
                movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
                movementController.SetPathfindingDestination(playerPosition);

                float distanceToPlayer = Vector3.Distance(_Character.transform.position, playerPosition);

                if (distanceToPlayer < closeFollowDistance)
                {
                    movementController.SetAllowMovement(false);
                }
                else
                {
                    movementController.SetAllowMovement(true);
                    if(distanceToPlayer < (midCirclingDistance - playerRangeSpan) || distanceToPlayer < (midCirclingDistance + playerRangeSpan))
                    {
                        movementState = VitriclawMovementState.MidRangeCircling;
                    }
                }
                break;
            case VitriclawMovementState.MidRangeCircling:
                if(!isCircling)
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
                    movementController.SetPathfindingDestination(movementController.pathfinding.destination + normal * midCirclingDistance + tangent * 4.5f);
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

                break;
            case VitriclawMovementState.MidRangeStrafe:

                break;
            case VitriclawMovementState.LongRangeStrafe:

                break;
        }

        if(movementController.rb.velocity == Vector3.zero && movementController.pathfinding.isStopped)
        {
            if (!stopped) bodyLayer.Play(idle);
            stopped = true;
        }
        else
        {
            if (stopped) bodyLayer.Play(moveBody);
            stopped = false;
        }
    }
}
