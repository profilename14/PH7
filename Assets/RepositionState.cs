using Animancer;
using PrimeTweenDemo;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class RepositionState : CharacterState
{
    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    [SerializeField]
    private ClipTransition repositionAnimation;

    [SerializeField]
    private float repositionDuration;

    [SerializeField]
    private float desiredDistanceFromPlayer;

    [SerializeField]
    private bool usePresetTransform;

    [SerializeField]
    private Transform presetTransform;

    private EnemyMovementController movementController;

    private bool startReposition = false;

    private float repositionProgress = 0;

    private float repositionTimer = 0;

    private Vector3 startPoint;

    private Vector3 endPoint;

    private void Awake()
    {
        base.Awake();

        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    private void OnEnable()
    {
        base.OnEnable();

        _ActionManager.SetAllActionPriorityAllowed(false);

        startReposition = false;

        AnimancerState currentState = _ActionManager.anim.Play(repositionAnimation);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

        startPoint = character.transform.position;
        repositionProgress = 0;
        repositionTimer = 0;

        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(true);
        movementController.SetForceLookAtPlayer(true);

        if (usePresetTransform) endPoint = presetTransform.position;
        else
        {
            endPoint = Player.instance.transform.position + ((character.transform.position - Player.instance.transform.position).normalized * desiredDistanceFromPlayer);
            endPoint = new Vector3(endPoint.x, character.transform.position.y, endPoint.z);
        }
    }

    private void OnDisable()
    {
        base.OnDisable();
        movementController.SetAllowRotation(true);
        movementController.SetAllowMovement(true);
        movementController.SetForceManualRotation(false);
        movementController.SetForceLookAtPlayer(false);
    }

    private void Update()
    {
        if (startReposition)
        {
            repositionTimer += Time.deltaTime;
            repositionProgress = repositionTimer / repositionDuration;

            if (repositionProgress > 1)
            {
                return;
            }

            Vector3 newPos = Vector3.Lerp(startPoint, endPoint, Mathf.Sqrt(repositionProgress));

            movementController.gameObject.transform.position = newPos;
        }
    }

    public void RepositionStart()
    {
        _Character.SetIsKnockbackImmune(true);
        startReposition = true;
    }

    public void RepositionEnd()
    {
        _Character.SetIsKnockbackImmune(false);
        startReposition = false;
    }
}
