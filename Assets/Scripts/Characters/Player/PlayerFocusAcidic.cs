using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class PlayerFocusAcidic : CharacterFocus
{
    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    private RotationController rotationController;

    [SerializeReference]
    ClipTransition focusAnimation;
    
    [SerializeReference]
    ClipTransition focusCastAnimation;

    public bool focusCharged = false;

    private AnimancerState currentState;



    // Uses allowedActions to control if entering this state is allowed.
    public override bool CanEnterState 
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Low];

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowed(false);

        rotationController.snapToCurrentMouseAngle();

        currentState = _ActionManager.anim.Play(focusAnimation);


        //currentState = actionManager.anim.Play(attackAnimations[currentSwing]);
        
        // Just sets to idle after this animation fully ends.
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

        focusCharged = false;

    }

    public void OnFinishFocus() {
        if (playerStats.armor >= playerStats.armorMax) {
            Debug.Log("Cannot heal past maximum armor");
            return;
        }
        
        playerStats.SetArmor(playerStats.armor + 1); // Will ask about how to formally set health during the meeting, playerstats just changed recently.
        Debug.Log("Player armor is now: " + playerStats.armor);
    }

    public void FocusDone() {
        focusCharged = true;
    }

    public override void ReleaseFocus()
    {
        if (focusCharged)
        {
            _ActionManager.SetAllActionPriorityAllowed(false);
            // Do a charge attack, go back to idle at the end.
            _ActionManager.anim.Play(focusCastAnimation).Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
        else
        {
            _ActionManager.StateMachine.ForceSetDefaultState();
        }
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
