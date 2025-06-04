using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class PlayerJump : CharacterState
{
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private PlayerActionManager actionManager;

    [SerializeField]
    TransitionAsset jumpAnimation;

    private PlayerDirectionalInput directionalInput;

    [Header("Jump Data")]
    public float jumpUpSpeed = 10f;
    public float maxJumpTime = 2f;
    public float jumpPreGroundingGraceTime = 0f;
    public float jumpPostGroundingGraceTime = 0f;
    [SerializeField]
    public AnimationCurve jumpSpeedCurve;
    [SerializeField]
    public AnimationCurve gravityCurve;

    private float jumpTimer;

    private bool canJump = true;

    public override bool CanEnterState
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Jump];

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
        movementController.PassJumpData(jumpUpSpeed, jumpPreGroundingGraceTime, jumpPostGroundingGraceTime);
    }

    protected override void OnEnable()
    {
        /*if (!movementController.IsGrounded())
        {
            _ActionManager.StateMachine.ForceSetDefaultState();
            return;
        }*/

        //Debug.Log(movementController.IsGrounded());
        _ActionManager.anim.Play(jumpAnimation);
        movementController.StartJump();
        jumpTimer = 0;
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);
        _ActionManager.SetAllActionPriorityAllowed(true);
    }

    protected void Update()
    {
        directionalInput = actionManager.GetDirectionalInput();
        movementController.RotateToDir(actionManager.GetDirRelativeToCamera(directionalInput.moveDir));

        movementController.SetJumpVelocity(jumpUpSpeed * jumpSpeedCurve.Evaluate(jumpTimer / maxJumpTime));
        movementController.SetGravityScale(gravityCurve.Evaluate(jumpTimer / maxJumpTime));

        jumpTimer += Time.deltaTime;

        if (!actionManager.IsJumpHeld() || jumpTimer > maxJumpTime)
        {
            movementController.StopJump();
            _ActionManager.StateMachine.ForceSetDefaultState();
        }
    }

    protected override void OnDisable()
    {
        movementController.SetGravityScale(1);
        movementController.StopJump();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
