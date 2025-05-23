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

    
    public override string StateName => "PlayerDash";


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

    [SerializeField] GameObject dashVFX;
    private GameObject instantiatedVFX;

    private PlayerVFXManager vfx;

    public override bool CanEnterState
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.High] && actionManager.dashTimer <= 0;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
        vfx = (PlayerVFXManager)character.VFXManager;
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

        if (dashVFX)
        {
            instantiatedVFX = Instantiate(dashVFX, transform);
            vfx.StartDashVFX();
        }
    }

    protected void Update()
    {
        dashTimer += Time.deltaTime;
        float dashProgress = dashTimer / duration;

        if (dashProgress > 1)
        {
            endDash();
            return;
        }

        if (!Physics.Raycast(transform.position, movementController.transform.forward, 2))
        {
            Vector3 newPos = Vector3.Lerp( startingPoint, destination, movementCurve.Evaluate(dashProgress) );

            movementController.SetPosition(newPos);
            
        }
        else
        {
            endDash();
        }

        if (actionManager.GetBufferedState() && actionManager.GetBufferedState().StateName == "PlayerDashAttack")
        {
            if (dashProgress < 0.5f)
            {
                Vector3 newPos = Vector3.Lerp( startingPoint, destination, movementCurve.Evaluate(dashProgress/2) ); // Go back slightly
                movementController.SetPosition(newPos);
                vfx.EndDashVFX();
                Destroy(instantiatedVFX, 0f);
                movementController.SetAllowRotation(true);
                _ActionManager.SetAllActionPriorityAllowed(true, 0);
                actionManager.ForceDashAttackState();
            }
        }
        
    }

    public override void endDash()
    {
        vfx.EndDashVFX();
        actionManager.EndDash(dashCooldown);
        isDashing = false;
        movementController.SetAllowRotation(true);
        movementController.SetVelocity(new Vector3(0, 10f, 0)); // Effectively very slightly reduces gravity for a moment
        _ActionManager.SetAllActionPriorityAllowed(true, 0);
        _ActionManager.StateMachine.ForceSetDefaultState();
    }



    protected override void OnDisable()
    {
        //PlayerCharacterInputs input = new();
        //movementController.SetInputs(ref input);

        if (instantiatedVFX)
        {
            Destroy(instantiatedVFX, 0.3f); // Give the vfx time to catch up to the player
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
