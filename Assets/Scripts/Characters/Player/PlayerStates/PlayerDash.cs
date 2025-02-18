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
    private RotationController rotationController;


    [SerializeField]
    private Vector3 moveDir;
    
    private Vector3 startingPoint;
    private Vector3 destination;

    [SerializeReference]
    ClipTransition dashAnimation;

    // float distance
    // float duration
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float dashTimer;
    
    [SerializeField]
    public AnimationCurve movementCurve;

    public bool isDashing = false;
    

    private AnimancerState currentState;

    private PlayerActionManager actionManager;
    private PlayerDirectionalInput directionalInput;

    public override bool CanEnterState
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.High] && actionManager.dashTimer <= 0;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
    }

    protected override void OnEnable()
    {
        directionalInput = actionManager.GetDirectionalInput();
        _ActionManager.SetAllActionPriorityAllowed(false);

        //movementController.rotateToMouse();
        //rotationController.snapToCurrentMouseAngle();
        //movementController.SetAllowRotation(false);
        //moveDir = movementController.GetMouseDirection();
        moveDir = actionManager.GetDirRelativeToCamera(directionalInput.moveDir);
        movementController.RotateToDir(moveDir);

        startingPoint = movementController.gameObject.transform.position;
        destination = new Vector3(startingPoint.x + moveDir.x * distance, 
                                  startingPoint.y,
                                  startingPoint.z + moveDir.z * distance);
        
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

        if (!Physics.Raycast(transform.position, movementController.transform.forward, 2))
        {
            Vector3 newPos = Vector3.Lerp( startingPoint, destination, Mathf.Sqrt(dashProgress) );

            movementController.SetPosition(newPos);
        }
        else
        {
            isDashing = false;
            endDash();
        }
        
    }

    public override void endDash()
    {
        actionManager.EndDash(dashCooldown);
        isDashing = false;
        movementController.SetAllowRotation(true);
        movementController.SetVelocity(new Vector3(0, 0, 0));
        _ActionManager.SetAllActionPriorityAllowed(true, 0);
        _ActionManager.StateMachine.ForceSetDefaultState();
    }



    protected override void OnDisable()
    {
        //PlayerCharacterInputs input = new();
        //movementController.SetInputs(ref input);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
