using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class EnemyFollowFlying : CharacterState
{
    [SerializeField]
    private TransitionAsset MoveAnimation;

    [SerializeField]
    private float followDistance = 2f;

    [SerializeField]
    private bool rotateToFaceMovementDirection = true;

    private EnemyMovementControllerFlying movementController;

    private Vector3 playerPosition;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowed(true);
        _ActionManager.anim.Play(MoveAnimation);
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
    }

    private void Update()
    {
        playerPosition = Player.instance.transform.position;
        movementController.SetPathfindingDestination(playerPosition);

        if (Vector3.Distance(_Character.transform.position, playerPosition) < followDistance)
        {
            movementController.SetAllowMovement(false);
        }
        else
        {
            movementController.SetAllowMovement(true);
        }
    }
}
