using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolSearch : EnemyPatrol
{
    protected Player player;

    [SerializeField]
    private Vector3 eyeOffset = new Vector3(0, 1, 0);

    [SerializeField]
    private float sightDistance = 20f;

    [SerializeField]
    // How wide the Enemy's full view cone is. 
    private float viewConeAngle = 90f;

    private LayerMask lineOfSightMask;

    private RoamingEnemyActionManager enemyActionManager;

    private void Awake()
    {
        base.Awake();
        enemyActionManager = (RoamingEnemyActionManager)_ActionManager;
        lineOfSightMask = LayerMask.GetMask("Player", "Obstacles");
    }

    private void Start()
    {
        player = Player.instance;
    }

    private void Update()
    {
        Vector3 toPlayer = (player.transform.position - _Character.transform.position).normalized;

        Physics.Raycast(transform.position + eyeOffset, toPlayer, 
            out RaycastHit hit, sightDistance, lineOfSightMask);

        Debug.DrawRay(transform.position + eyeOffset, toPlayer * sightDistance);

        if (hit.collider != null && hit.collider.gameObject == player.gameObject)
        {
            if (Vector3.Angle(_Character.transform.forward, toPlayer) < viewConeAngle / 2)
            {
                StopAllCoroutines();
                enemyActionManager.SpottedPlayer();
            }
        }
    }
}
