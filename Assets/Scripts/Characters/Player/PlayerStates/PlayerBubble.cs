using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class PlayerBubble : CharacterSpell
{
    [SerializeField]
    private PlayerStats playerStats;
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private RotationController rotationController;
    [SerializeField]
    private PlayerActionManager actionManager;

    [SerializeReference]
    ClipTransition focusAnimation;


    [SerializeField]
    GameObject bubblePrefab;


    private AnimancerState currentState;

    PlayerDirectionalInput directionalInput;



    // Uses allowedActions to control if entering this state is allowed.
    public override bool CanEnterState 
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Low];

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowed(false);

        directionalInput = actionManager.GetDirectionalInput();

        movementController.RotateToDir(directionalInput.lookDir);

        currentState = _ActionManager.anim.Play(focusAnimation);


        //currentState = actionManager.anim.Play(attackAnimations[currentSwing]);
        
        // Just sets to idle after this animation fully ends.
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

    }

    public void OnFinishCast() {
        Vector3 ArrowLocation = transform.position + transform.forward;

        Vector3 curRotation = transform.forward;
        float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

        GameObject bubbleObject = Instantiate(bubblePrefab, ArrowLocation, Quaternion.Euler(0, angle, 0) );
        
        FloatingBubble bubble = bubbleObject.GetComponent<FloatingBubble>();

        if (bubble != null)
        {
            bubble.force = 35;
            bubble.direction = transform.forward;
        }




        
        movementController.SetAllowRotation(true);
        _ActionManager.SetAllActionPriorityAllowed(true, 0);
        _ActionManager.StateMachine.ForceSetDefaultState();
    }



#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref playerStats);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
    }
#endif
}
