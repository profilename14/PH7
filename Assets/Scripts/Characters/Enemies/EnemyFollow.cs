using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Pathfinding;

public class EnemyFollow : CharacterState
{
    [SerializeField]
    private TransitionAsset MoveAnimation;

    [SerializeField]
    private float followDistance = 2f;

    [SerializeField]
    private float stationaryRotationMultiplier = 5f;

    private EnemyActionManager enemyActionManager;

    private RichAI pathfinding;

    private void Awake()
    {
        enemyActionManager = (EnemyActionManager)actionManager;
        pathfinding = enemyActionManager.pathfinding;
    }

    protected override void OnEnable()
    {
        actionManager.anim.Play(MoveAnimation);
        pathfinding.isStopped = false;
        pathfinding.enableRotation = true;
        pathfinding.endReachedDistance = followDistance;
        pathfinding.maxSpeed = character.characterData.baseSpeed;
    }

    private void Update()
    {
        enemyActionManager.target.transform.position = Player.instance.transform.position;

        if(pathfinding.reachedEndOfPath)
        {
            pathfinding.isStopped = true;
            var direction = (enemyActionManager.target.transform.position - character.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, lookRotation, Time.deltaTime * stationaryRotationMultiplier);
        }
        else
        {
            pathfinding.isStopped = false;
        }
    }
}
