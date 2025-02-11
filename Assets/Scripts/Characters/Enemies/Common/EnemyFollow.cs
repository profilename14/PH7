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
    private bool rotateToFaceMovementDirection = true;

    private EnemyMovementController movementController;

    private Vector3 playerPosition;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    protected override void OnEnable()
    {
        playerPosition = Player.instance.transform.position;
        _ActionManager.SetAllActionPriorityAllowed(true);
        _ActionManager.anim.Play(MoveAnimation);
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(!rotateToFaceMovementDirection);
    }

    private void Update()
    {
        movementController.SetPathfindingDestination(playerPosition);

        if(Vector3.Distance(_Character.transform.position, playerPosition) < followDistance)
        {
            movementController.SetAllowMovement(false);
        }
        else
        {
            movementController.SetAllowMovement(true);
        }
    }
}
