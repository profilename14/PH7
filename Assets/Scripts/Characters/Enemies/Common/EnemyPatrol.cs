using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer.FSM;
using Animancer;

public class EnemyPatrol : CharacterState
{
    [SerializeField]
    private TransitionAsset IdleAnimation;

    [SerializeField]
    private TransitionAsset MoveAnimation;

    [SerializeField]
    // How long this Enemy will pause at each patrol target before moving on to the next one.
    private float patrolPauseTime = 2f;

    [SerializeField]
    // If this Enemy will randomly choose the next patrol target instead of choosing them sequentially.
    private bool randomlyChooseTargets;

    [SerializeField]
    // A list of GameObject targets the Enemy can visit while patrolling.
    public GameObject[] patrolTargets;

    private int currentPatrolIndex;

    EnemyMovementController movementController;

    private void Awake()
    {
    }

    protected override void OnEnable()
    {
        gameObject.GetComponentInParentOrChildren(ref _Character);
        gameObject.GetComponentInParentOrChildren(ref movementController);
        _ActionManager = _Character.actionManager;
        _MovementController = _Character.movementController;

        // This is a roaming enemy
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);

        if(randomlyChooseTargets) currentPatrolIndex = Random.Range(0, patrolTargets.Length);

        StopAllCoroutines();
        StartCoroutine(Patrolling());
    }

    private IEnumerator Patrolling()
    {
        yield return null;

        //Debug.Log("Patrolling, destination " + currentPatrolIndex);

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

    public void InitPatrolPoints(int length, GameObject[] points)
    {
        patrolTargets = new GameObject[length];
        for(int i = 0; i < length; i++)
        {
            patrolTargets[i] = points[i];
        }
    }
}
