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

    [SerializeField] bool setupLayers;

    private AnimancerLayer bodyLayer;

    EnemyActionManager actionManager;

    private void Awake()
    {
        base.Awake();
        actionManager = (EnemyActionManager)_ActionManager;
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    protected override void OnEnable()
    {
        //Debug.Log("Entering follow");

        _ActionManager.SetAllActionPriorityAllowed(true);
        
        if (setupLayers)
        {
            bodyLayer = actionManager.anim.Layers[0];
            bodyLayer.Play(MoveAnimation);
        }
        else
        {
            _ActionManager.anim.Play(MoveAnimation);
        }

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(!rotateToFaceMovementDirection);
    }

    private void Update()
    {
        _ActionManager.SetAllActionPriorityAllowed(true);
        playerPosition = Player.instance.transform.position;
        movementController.SetPathfindingDestination((character.transform.position - playerPosition).normalized * followDistance);

        /* if(Vector3.Distance(_Character.transform.position, playerPosition) < followDistance)
        {
            movementController.SetAllowMovement(false);
        }
        else
        {
            movementController.SetAllowMovement(true);
        }*/
    }
}
