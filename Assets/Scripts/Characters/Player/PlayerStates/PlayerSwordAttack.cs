using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public enum SwordSwingType { Swing0, Swing1, Swing2, SwingDown, ChargedSwing }

public class PlayerSwordAttack : AttackState
{
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private RotationController rotationController;

    // Just start at maxValue so it will always have first swing at 0.
    int currentSwing = int.MaxValue;

    [SerializeField, EventNames]
    ClipTransition[] attackAnimations;

    [SerializeField, EventNames]
    ClipTransition downwardsSwingAnimation;

    private static readonly StringReference StartSwordSwingEvent = "StartSwordSwing";
    private static readonly StringReference EndSwordSwingEvent = "EndSwordSwing";

    [SerializeField]
    AudioClip swordSwingSFX;

    [SerializeField]
    float swingForce = 2f;

    [SerializeField]
    float hitRecoilForce;

    [SerializeField]
    float pogoForce;

    [SerializeField]
    float startNewComboCooldown = 0.2f;

    [SerializeField]
    float drag;

    private bool isPogo;

    private AnimancerState currentState;

    private PlayerVFXManager vfx;

    private Player player;

    private PlayerActionManager actionManager;

    private SwordSwingType currentSwordSwing;

    private PlayerDirectionalInput directionalInput = new PlayerDirectionalInput();

    private Vector3 moveDirAtStart; 

    // Uses allowedActions to control if entering this state is allowed.
    // Also must have animations in the array.
    public override bool CanEnterState 
        => attackAnimations.Length > 0 && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    protected virtual void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);

        player = (Player)_Character;
        vfx = (PlayerVFXManager)player.VFXManager;

        for(int i = 0; i < attackAnimations.Length; i++)
        {
            attackAnimations[i].Events.SetCallback(StartSwordSwingEvent, this.StartSwordSwing);
            attackAnimations[i].Events.SetCallback(EndSwordSwingEvent, this.EndSwordSwing);
            attackAnimations[i].Events.SetCallback(AllowHighPriorityEvent, AllowHighPriority);
        }

        downwardsSwingAnimation.Events.SetCallback(StartSwordSwingEvent, StartSwordSwing);
        downwardsSwingAnimation.Events.SetCallback(EndSwordSwingEvent, EndSwordSwing);
        downwardsSwingAnimation.Events.SetCallback(AllowHighPriorityEvent, AllowHighPriority);
    }

    protected override void OnEnable()
    {
        directionalInput = actionManager.GetDirectionalInput();

        moveDirAtStart = directionalInput.moveDir;

        // Fully committed to an attack once you start it.
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Medium, false);
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);

        movementController.RotateToDir(directionalInput.lookDir);

        if (movementController.IsGrounded())
        {
            movementController.SetGroundDrag(drag);
            movementController.AddVelocity(actionManager.GetDirRelativeToCamera(moveDirAtStart) * swingForce);

            isPogo = false;

            movementController.SetAllowMovement(false);
            movementController.SetAllowRotation(false);

            if (currentSwing >= attackAnimations.Length - 1 || currentState == null || currentState.Weight == 0)
            {
                currentSwing = 0;
            }
            else
            {
                currentSwing++;
            }

            //Debug.Log("Current swing: " + currentSwing);

            currentSwordSwing = (SwordSwingType)currentSwing;

            currentState = _ActionManager.anim.Play(attackAnimations[currentSwing]);

            if(currentSwing == 2) _ActionManager.SetAllActionPriorityAllowed(true, currentState.Duration);

            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
        else
        {
            isPogo = true;

            currentSwordSwing = SwordSwingType.SwingDown;
            movementController.SetAllowRotation(true);

            // Swinging in the air performs a downwards swing.
            currentState = _ActionManager.anim.Play(downwardsSwingAnimation);

            _ActionManager.SetAllActionPriorityAllowed(true, currentState.Duration);

            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
    }



    protected void Update()
    {
        movementController.RotateToDir(directionalInput.lookDir);
    }

    public override void OnAttackHit(Vector3 position)
    {
        vfx.SwordHitVFX(position);

        if(currentSwordSwing == SwordSwingType.SwingDown)
        {
            movementController.SetVelocity(Vector3.zero);
            movementController.SetVelocity(movementController.gameObject.transform.transform.up * pogoForce);
        }
        else
        {
            movementController.SetVelocity(movementController.gameObject.transform.transform.forward * hitRecoilForce);
        }
    }

    public void StartSwordSwing()
    {
        vfx.SwordSwingVFX(currentSwordSwing);
    }

    public void EndSwordSwing()
    {
        AllowMove();
        AllowJump();
        AllowLowPriority();
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);

        if(!isPogo && currentSwing >= attackAnimations.Length - 1)
        {
            //Debug.Log("resetting");
            currentSwing = -1;
            // Doing it like this instead of events, more consistency if exiting state
            actionManager.SetActionPriorityAllowed(CharacterActionPriority.Medium, startNewComboCooldown);
        }
        else
        {
            AllowMediumPriority();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        movementController.ResetDrag();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
