using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class PlayerAcidArrow : CharacterSpell
{
    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    private RotationController rotationController;

    [SerializeReference]
    ClipTransition focusAnimation;


    [SerializeField]
    GameObject projectilePrefab;


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

    }

    public void OnFinishCast() {
        Vector3 ArrowLocation = transform.position + transform.forward;

        Vector3 curRotation = transform.forward;
        float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

        GameObject arrow = Instantiate(projectilePrefab, ArrowLocation, Quaternion.Euler(0, angle, 0) );

        Projectile projectileScript = arrow.GetComponent<Projectile>();

        projectileScript.sender = character;
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
