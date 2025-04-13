using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using Animancer;
using Animancer.FSM;

public class PlayerMove : CharacterState
{
    [SerializeField]
    private PlayerMovementController movementController;
    
    [SerializeField]
    private PlayerActionManager actionManager;

    [SerializeField]
    private PlayerVFXManager playerVFXManager;

    [SerializeField]
    TransitionAsset idleAnimation;

    [SerializeField]
    TransitionAsset moveAnimation;

    [SerializeField]
    TransitionAsset fallAnimation;

    [SerializeField]
    TransitionAsset landAnimation;


    private PlayerDirectionalInput directionalInput;

    private bool wasGrounded;

    private bool isLanding;

    public override bool CanEnterState
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Move];

    private bool isPlayingRunVFX = false;


    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
    }
    protected override void OnEnable()
    {
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);

        playerVFXManager.StartRunVFX(gameObject.transform);
        isPlayingRunVFX = true;
    }

    protected void Update()
    {
        actionManager.SetActionPriorityAllowed(CharacterActionPriority.Jump, movementController.IsAbleToJump());

        directionalInput = actionManager.GetDirectionalInput();

        if(directionalInput.usingController)
        {
            movementController.RotateToDir(directionalInput.lookDir);
        }
        else
        {
            movementController.RotateToDir(actionManager.GetDirRelativeToCamera(directionalInput.moveDir));
        }
        
        if (movementController.IsGrounded())
        {
            if (!wasGrounded)
            {
                isLanding = true;
                _ActionManager.anim.Play(landAnimation).Events(this).OnEnd ??= () => { isLanding = false; };
            }
            else if(!isLanding)
            {
                if (directionalInput.moveDir.magnitude == 0)
                {
                    _ActionManager.anim.Play(idleAnimation);
                }
                else
                {
                    _ActionManager.anim.Play(moveAnimation);
                }
            }

            wasGrounded = true;
        }
        else
        {
            wasGrounded = false;
            _ActionManager.anim.Play(fallAnimation);
        }

        if (!isPlayingRunVFX)
        {
            if (movementController.IsGrounded() && directionalInput.moveDir.magnitude > 0)
            {
                playerVFXManager.StartRunVFX(gameObject.transform);
                isPlayingRunVFX = true;
            }
        }
        else
        {
            if (!movementController.IsGrounded() || directionalInput.moveDir.magnitude <= 0)
            {
                playerVFXManager.StopRunVFX();
                isPlayingRunVFX = false;
            }
        }
    }

    protected override void OnDisable()
    {
        isLanding = false;

        playerVFXManager.StopRunVFX();
        isPlayingRunVFX = false;
    }

/*#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
    }
#endif*/
}
