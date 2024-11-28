using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer.FSM;
using Animancer;

public class EnemyIdle : CharacterState
{
    [SerializeField]
    private TransitionAsset IdleAnimation;

    [SerializeField]
    private TransitionAsset MoveAnimation;

    [SerializeField]
    private Vector3 eyeOffset = new Vector3(0, 1, 0);

    [SerializeField]
    private float sightDistance = 20f;

    [SerializeField]
    // How wide the Enemy's full view cone is. 
    private float viewConeAngle = 90f;

    [SerializeField]
    // How long this Enemy will pause at each patrol target before moving on to the next one.
    private float patrolPauseTime = 2f;

    [SerializeField]
    // If this Enemy will randomly choose the next patrol target instead of choosing them sequentially.
    private bool randomlyChooseTargets;

    [SerializeField]
    // A list of GameObject targets the Enemy can visit while patrolling.
    private GameObject[] patrolTargets;

    private LayerMask lineOfSightMask;

    private Player player;

    private int currentPatrolIndex;

    EnemyMovementController movementController;

    EnemyActionManager actionManager;

    private void Awake()
    {
        lineOfSightMask = LayerMask.GetMask("Player", "Obstacles");
        //actionManager = (EnemyActionManager) 
    }

    private void Start()
    {
        player = Player.instance;
    }

    protected override void OnEnable()
    {
        movementController = (EnemyMovementController)_Character.movementController;

        if (patrolTargets.Length > 0)
        {
            // This is a roaming enemy
            movementController.SetAllowMovement(true);
            movementController.SetAllowRotation(true);

            if(randomlyChooseTargets) currentPatrolIndex = Random.Range(0, patrolTargets.Length);

            StartCoroutine(Patrolling());
        }
        else
        {
            // This is a stationary enemy
            movementController.SetAllowMovement(false);
            movementController.SetAllowRotation(false);
        }
    }

    private void Update()
    {
        //if(_ActionManager.)

        Vector3 toPlayer = (player.transform.position - _Character.transform.position).normalized;

        Physics.Raycast(transform.position + eyeOffset, toPlayer, 
            out RaycastHit hit, sightDistance, lineOfSightMask);

        Debug.DrawRay(transform.position + eyeOffset, toPlayer * sightDistance);

        if (hit.collider != null && hit.collider.gameObject == player.gameObject)
        {
            if (Vector3.Angle(_Character.transform.forward, toPlayer) < viewConeAngle / 2)
            {
                StopAllCoroutines();
                EnemyActionManager enemyActionManager = (EnemyActionManager)_ActionManager;
                enemyActionManager.SpottedPlayer();
            }
        }
    }

    private IEnumerator Patrolling()
    {
        yield return null;

        _ActionManager.anim.Play(MoveAnimation);

        movementController.SetPathfindingDestination(patrolTargets[currentPatrolIndex].transform.position);

        yield return null;

        while(!movementController.ReachedDestination())
        {
            yield return null;
        }

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
