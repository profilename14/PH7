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

    [SerializeField]
    AudioClip swordSwingSFX;

    // Forward force feels weird and makes you go woosh in the air
    //[SerializeField]
    //float forwardForce = 2f;

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

        // Forward force was weird in the air so it's disabled for now.
        // movementController.AddVelocity(rotationController.gameObject.transform.right * forwardForce);

        if(currentSwing >= attackAnimations.Length - 1 || currentState == null || currentState.Weight == 0)
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

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
    }
#endif
}
