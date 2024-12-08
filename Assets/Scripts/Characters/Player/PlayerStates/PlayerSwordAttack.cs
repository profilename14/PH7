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

    private AnimancerState currentState;

    private PlayerVFXManager vfx;

    private Player player;

    private SwordSwingType currentSwordSwing;

    // Uses allowedActions to control if entering this state is allowed.
    // Also must have animations in the array.
    public override bool CanEnterState 
        => attackAnimations.Length > 0 && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    protected virtual void Awake()
    {
        player = (Player)_Character;
        vfx = (PlayerVFXManager)player.VFXManager;

        for(int i = 0; i < attackAnimations.Length; i++)
        {
            attackAnimations[i].Events.SetCallback(StartSwordSwingEvent, this.StartSwordSwing);
            attackAnimations[i].Events.SetCallback(EndSwordSwingEvent, this.EndSwordSwing);
            attackAnimations[i].Events.SetCallback(AllowHighPriorityEvent, AllowHighPriority);
            attackAnimations[i].Events.SetCallback(AllowMediumPriorityEvent, AllowMediumPriority);
            attackAnimations[i].Events.SetCallback(AllowLowPriorityEvent, AllowLowPriority);
            attackAnimations[i].Events.SetCallback(AllowMoveStateEvent, AllowMove);
            //attackAnimations[i].Events.AddCallback<bool>(AllowMovementEvent, AllowMovement);
            //attackAnimations[i].Events.AddCallback<bool>(AllowRotationEvent, AllowRotation);
        }

        downwardsSwingAnimation.Events.SetCallback(StartSwordSwingEvent, StartSwordSwing);
        downwardsSwingAnimation.Events.SetCallback(EndSwordSwingEvent, EndSwordSwing);
        downwardsSwingAnimation.Events.SetCallback(AllowHighPriorityEvent, AllowHighPriority);
        downwardsSwingAnimation.Events.SetCallback(AllowMediumPriorityEvent, AllowMediumPriority);
        downwardsSwingAnimation.Events.SetCallback(AllowLowPriorityEvent, AllowLowPriority);
        downwardsSwingAnimation.Events.SetCallback(AllowMoveStateEvent, AllowMove);
        //downwardsSwingAnimation.Events.AddCallback<bool>(AllowMovementEvent, AllowMovement);
        //downwardsSwingAnimation.Events.AddCallback<bool>(AllowRotationEvent, AllowRotation);
    }

    protected override void OnEnable()
    {
        // Fully committed to an attack once you start it.
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);

        rotationController.snapToCurrentMouseAngle();

        if (movementController.IsGrounded())
        {
            // Swinging on the ground
            movementController.AddVelocity(rotationController.gameObject.transform.right * swingForce);

            if (currentSwing >= attackAnimations.Length - 1 || currentState == null || currentState.Weight == 0)
            {
                currentSwing = 0;
            }
            else
            {
                currentSwing++;
            }

            currentSwordSwing = (SwordSwingType)currentSwing;

            currentState = _ActionManager.anim.Play(attackAnimations[currentSwing]);

            if(currentSwing == 2) _ActionManager.SetAllActionPriorityAllowed(true, currentState.Duration);

            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
        else
        {
            currentSwordSwing = SwordSwingType.SwingDown;

            //movementController.AddVelocity(-rotationController.gameObject.transform.up * swingForce);

            // Swinging in the air performs a downwards swing.
            currentState = _ActionManager.anim.Play(downwardsSwingAnimation);

            _ActionManager.SetAllActionPriorityAllowed(true, currentState.Duration);

            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
    }

    public void UpdateInputs(PlayerCharacterInputs input)
    {
        if (currentSwordSwing == SwordSwingType.SwingDown) movementController.SetInputs(ref input);
    }

    public override void OnAttackHit(Vector3 position)
    {
        vfx.SwordHitVFX(position);

        if(currentSwordSwing == SwordSwingType.SwingDown)
        {
            //Debug.Log("Pogo!");
            movementController.SetVelocity(Vector3.zero);
            movementController.AddVelocity(rotationController.gameObject.transform.up * pogoForce);
        }
        else
        {
            //movementController.AddVelocity(-rotationController.gameObject.transform.right * hitRecoilForce);
        }
    }

    public void StartSwordSwing()
    {
        movementController.AddVelocity(rotationController.gameObject.transform.right * swingForce);

        vfx.SwordSwingVFX(currentSwordSwing);
    }

    public void EndSwordSwing()
    {
        //Debug.Log("Ending swing");
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
    }
#endif
}
