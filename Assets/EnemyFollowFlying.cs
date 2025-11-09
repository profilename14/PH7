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

    [SerializeField]
    private bool allowBackingUp;

    [SerializeField]
    private float maxYDiffHeight;

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
        movementController.SetForceLookRotation(true);
    }

    private void Update()
    {
        playerPosition = Player.instance.transform.position;

        if (Vector3.Distance(_Character.transform.position, playerPosition) < followDistance)
        {
            if(!allowBackingUp) movementController.SetAllowMovement(false);
            else
            {
                Vector3 followTarget = playerPosition + ((_Character.transform.position - playerPosition).normalized * followDistance);
                if(followTarget.y - playerPosition.y > maxYDiffHeight) followTarget.y = playerPosition.y + maxYDiffHeight;
                movementController.SetPathfindingDestination(followTarget);
            }
        }
        else
        {
            movementController.SetAllowMovement(true);
            movementController.SetPathfindingDestination(playerPosition);
        }
    }
}
