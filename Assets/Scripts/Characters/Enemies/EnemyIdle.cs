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

    private EnemyActionManager enemyActionManager;

    private Player player;

    private int currentPatrolIndex;

    private void Awake()
    {
        lineOfSightMask = LayerMask.GetMask("Player", "Obstacles");
        enemyActionManager = (EnemyActionManager)actionManager;
    }

    protected override void OnEnable()
    {
        player = Player.instance;

        if (patrolTargets.Length > 0)
        {
            // This is a roaming enemy
            enemyActionManager.pathfinding.isStopped = false;
            enemyActionManager.pathfinding.enableRotation = true;

            StartCoroutine(Patrolling());
        }
        else
        {
            // This is a stationary enemy
            enemyActionManager.pathfinding.isStopped = true;
            enemyActionManager.pathfinding.enableRotation = false;
        }
    }

    private void Update()
    {
        Vector3 toPlayer = (player.transform.position - character.transform.position).normalized;

        Physics.Raycast(transform.position + eyeOffset, toPlayer, 
            out RaycastHit hit, sightDistance, lineOfSightMask);

        Debug.DrawRay(transform.position + eyeOffset, toPlayer * sightDistance);

        if (hit.collider != null && hit.collider.gameObject == player.gameObject)
        {
            if (Vector3.Angle(character.transform.forward, toPlayer) < viewConeAngle / 2)
            {
                StopAllCoroutines();
                enemyActionManager.SpottedPlayer();
            }
        }
    }

    private IEnumerator Patrolling()
    {
        actionManager.anim.Play(MoveAnimation);

        enemyActionManager.target.transform.position = patrolTargets[currentPatrolIndex].transform.position;

        yield return null;

        while(!enemyActionManager.pathfinding.reachedDestination)
        {
            yield return null;
        }

        actionManager.anim.Play(IdleAnimation);

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
