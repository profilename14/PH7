using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class EnemyFollowFlying : CharacterState
{
    [SerializeField]
    private TransitionAsset IdleAnimation;

    [SerializeField]
    private TransitionAsset MoveAnimation;

    [SerializeField]
    private float followDistance = 2f;

    private EnemyMovementControllerFlying movementController;

    private Vector3 playerPosition;

    [SerializeField]
    private Vector3 playerTargetOffset;

    [SerializeField]
    private bool allowBackingUp;

    [SerializeField]
    private float maxYDiffHeight;

    [SerializeField]
    private bool hasSpottedPlayer;

    [SerializeField]
    private Vector3 eyeOffset = new Vector3(0, 1, 0);

    [SerializeField]
    private float sightDistance = 20f;

    [SerializeField]
    // How wide the Enemy's full view cone is. 
    private float viewConeAngle = 90f;

    private LayerMask lineOfSightMask;

    [SerializeField]
    // How long this Enemy will pause at each patrol target before moving on to the next one.
    private float patrolPauseTime = 2f;

    [SerializeField]
    // If this Enemy will randomly choose the next patrol target instead of choosing them sequentially.
    private bool randomlyChooseTargets;

    [SerializeField]
    // A list of GameObject targets the Enemy can visit while patrolling.
    public GameObject[] patrolTargets;

    [SerializeField]
    public float patrolTargetReachedDistance;

    [SerializeField]
    public bool isPatrolling = true;

    private int currentPatrolIndex;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        lineOfSightMask = LayerMask.GetMask("Player", "Obstacles");
        if (patrolTargets.Length <= 1) isPatrolling = false;
    }

    protected override void OnEnable()
    {
        Debug.Log("Entered enemy follow flying!");
        base.OnEnable();
        _ActionManager.SetAllActionPriorityAllowed(true);
        _ActionManager.anim.Play(MoveAnimation);
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceLookRotation(true);

        if (randomlyChooseTargets) currentPatrolIndex = Random.Range(0, patrolTargets.Length);

        StopAllCoroutines();

        if (isPatrolling && !hasSpottedPlayer)
        {
            movementController.SetForceLookRotation(false);
            StartCoroutine(Patrolling());
        }
    }

    private void Update()
    {
        playerPosition = Player.instance.transform.position;

        if (hasSpottedPlayer)
        {
            if (Vector3.Distance(_Character.transform.position, playerPosition) < followDistance)
            {
                if (!allowBackingUp) movementController.SetAllowMovement(false);
                else
                {
                    Vector3 followTarget = playerPosition + ((_Character.transform.position - playerPosition).normalized * followDistance) + playerTargetOffset;
                    if (followTarget.y - playerPosition.y > maxYDiffHeight) followTarget.y = playerPosition.y + maxYDiffHeight + playerTargetOffset.y;
                    movementController.SetPathfindingDestination(followTarget);
                }
            }
            else
            {
                movementController.SetAllowMovement(true);
                movementController.SetPathfindingDestination(playerPosition + playerTargetOffset);
            }
        }
        else
        {
            Vector3 toPlayer = (playerPosition - _Character.transform.position).normalized;

            Physics.Raycast(_Character.transform.position + eyeOffset, toPlayer,
                out RaycastHit hit, sightDistance, lineOfSightMask);

            Debug.DrawRay(_Character.transform.position + eyeOffset, toPlayer * sightDistance);

            if (hit.collider != null && hit.collider.gameObject == Player.instance.gameObject)
            {
                if (Vector3.Angle(_Character.transform.forward, toPlayer) < viewConeAngle / 2)
                {
                    isPatrolling = false;
                    hasSpottedPlayer = true;
                    Debug.Log("Spotted player!");
                    StopAllCoroutines();
                    movementController.SetForceLookRotation(true);
                    movementController.SetAllowMovement(true);
                    movementController.SetAllowRotation(true);
                    _ActionManager.StartCoroutine("UpdateAttackStates");
                }
            }
        }
    }

    private IEnumerator Patrolling()
    {
        yield return null;

        Debug.Log("Patrolling, destination " + currentPatrolIndex);

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);

        _ActionManager.anim.Play(MoveAnimation);

        movementController.SetPathfindingDestination(patrolTargets[currentPatrolIndex].transform.position);

        yield return null;

        while (Vector3.Distance(_Character.transform.position, patrolTargets[currentPatrolIndex].transform.position) > patrolTargetReachedDistance)
        {
            yield return null;
        }

        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(false);

        _ActionManager.anim.Play(IdleAnimation);

        yield return new WaitForSeconds(patrolPauseTime);

        if (!randomlyChooseTargets)
        {
            if (currentPatrolIndex == patrolTargets.Length - 1) currentPatrolIndex = 0;
            else currentPatrolIndex++;
        }
        else
        {
            currentPatrolIndex = Random.Range(0, patrolTargets.Length);
        }

        StartCoroutine(Patrolling());
    }
}
