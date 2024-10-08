using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class PlayerFocusSalt : CharacterFocus
{
    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    private RotationController rotationController;

    [SerializeReference]
    ClipTransition focusAnimation;
    
    [SerializeReference]
    ClipTransition focusCastAnimation;


    [SerializeField]
    GameObject shieldPrefab;

    public bool focusCharged = false; // modified by actionmanager


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
        Vector3 ShieldLocation = transform.position + transform.forward * 2.5f;

        Vector3 curRotation = transform.forward;
        float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

        GameObject shield = Instantiate(shieldPrefab, ShieldLocation, Quaternion.Euler(0, angle, 0) );

        SaltShield saltShieldScript = shield.GetComponent<SaltShield>();

        saltShieldScript.playerStats = playerStats;

        Debug.Log("Created Salt Shield");
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
