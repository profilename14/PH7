using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using Animancer;
using Animancer.FSM;

public class PlayerDash : DashState
{
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private Vector3 moveDir;
    
    private Vector3 startingPoint;
    private Vector3 destination;

    [SerializeReference]
    ClipTransition dashAnimation;

    // float distance
    // float duration
    [SerializeField] private float dashTimer;
    public bool isDashing = false;
    

    private AnimancerState currentState;

    public override bool CanEnterState
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Low];

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowed(false);

        movementController.rotateToMouse();
        movementController.SetAllowRotation(false);
        moveDir = movementController.GetMouseDirection();

        startingPoint = movementController.gameObject.transform.position;
        destination = new Vector3(startingPoint.x - moveDir.x * distance, 
                                  startingPoint.y,
                                  startingPoint.z - moveDir.z * distance);
        
        // Just sets to idle after this animation fully ends.
        currentState = _ActionManager.anim.Play(dashAnimation);
        //currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

        dashTimer = 0;
        isDashing = true;
    }

    protected void Update()
    {
        dashTimer += Time.deltaTime;
        float dashProgress = dashTimer / duration;

        if (dashProgress > 1)
        {
            isDashing = false;
            endDash();
            return;
        }

        Vector3 newPos = Vector3.Lerp( startingPoint, destination, Mathf.Sqrt(Mathf.Sqrt(dashProgress)) );

        movementController.SetPosition(newPos);
    }

    public override void endDash()
    {
        isDashing = false;
        movementController.SetAllowRotation(true);
        _ActionManager.SetAllActionPriorityAllowed(true, 0);
        _ActionManager.StateMachine.ForceSetDefaultState();
    }

    protected override void OnDisable()
    {
        PlayerCharacterInputs input = new();
        movementController.SetInputs(ref input);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }
#endif
}
