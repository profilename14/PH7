using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class PlayerSwordAttack : AttackState
{
    [SerializeField]
    private ICharacterMovementController movementController;

    [SerializeField]
    private RotationController rotationController;

    // Just start at maxValue so it will always have first swing at 0.
    int currentSwing = int.MaxValue;

    [SerializeReference]
    ClipTransition[] attackAnimations;

    [SerializeReference]
    ClipTransition downwardsSwingAnimation;

    [SerializeField]
    AudioClip swordSwingSFX;

    [SerializeField]
    float swingForce = 2f;

    [SerializeField]
    float hitRecoilForce;

    [SerializeField]
    float pogoForce;

    private AnimancerState currentState;

    // Uses allowedActions to control if entering this state is allowed.
    // Also must have animations in the array.
    public override bool CanEnterState 
        => attackAnimations.Length > 0 && actionManager.allowedActions[this];

    protected override void OnEnable()
    {
        // Fully committed to an attack once you start it.
        // May want to change this later so you can be hit out of attacks.
        actionManager.SetAllActionsAllowed(false);

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

            currentState = actionManager.anim.Play(attackAnimations[currentSwing]);

            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= actionManager.StateMachine.ForceSetDefaultState;
        }
        else
        {
            //movementController.AddVelocity(-rotationController.gameObject.transform.up * swingForce);

            // Swinging in the air performs a downwards swing.
            currentState = actionManager.anim.Play(downwardsSwingAnimation);

            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= actionManager.StateMachine.ForceSetDefaultState;
        }
    }

    public override void OnAttackHit()
    {
        if(currentState.Clip == downwardsSwingAnimation.Clip)
        {
            Debug.Log("Pogo!");
            movementController.AddVelocity(rotationController.gameObject.transform.up * pogoForce);
        }
        else
        {
            //movementController.AddVelocity(-rotationController.gameObject.transform.right * hitRecoilForce);
        }
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
