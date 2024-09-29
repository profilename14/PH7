using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class PlayerSwordAttack : AttackState
{
    [SerializeField]
    private PlayerMovementController movementController;

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
        => attackAnimations.Length > 0 && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

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

            currentState = _ActionManager.anim.Play(attackAnimations[currentSwing]);

            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
        else
        {
            //movementController.AddVelocity(-rotationController.gameObject.transform.up * swingForce);

            // Swinging in the air performs a downwards swing.
            currentState = _ActionManager.anim.Play(downwardsSwingAnimation);

            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
    }

    public void UpdateInputs(PlayerCharacterInputs input)
    {
        if (currentState.Clip == downwardsSwingAnimation.Clip) movementController.SetInputs(ref input);
    }

    public override void OnAttackHit()
    {
        if(currentState.Clip == downwardsSwingAnimation.Clip)
        {
            Debug.Log("Pogo!");
            movementController.SetVelocity(Vector3.zero);
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
