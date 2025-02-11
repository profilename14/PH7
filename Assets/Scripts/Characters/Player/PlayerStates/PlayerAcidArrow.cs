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
    private PlayerMovementController movementController;

    [SerializeField]
    private RotationController rotationController;
    [SerializeField]
    private PlayerActionManager actionManager;

    [SerializeReference]
    ClipTransition focusAnimation;


    [SerializeField]
    GameObject projectilePrefab;

    [SerializeField]
    AttackData acidArrowStats;

    private AnimancerState currentState;

    PlayerDirectionalInput directionalInput;



    // Uses allowedActions to control if entering this state is allowed.
    public override bool CanEnterState 
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Low] && playerStats.acid >= acidCost;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref playerStats);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
    }

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

        GameObject arrow = Instantiate(projectilePrefab, ArrowLocation, Quaternion.Euler(0, 0, 0) );

        MyProjectile projectileScript = arrow.GetComponent<MyProjectile>();

        if (projectileScript != null)
        {
            projectileScript.InitProjectile(ArrowLocation, new Vector3(0, angle, 0), character, acidArrowStats);
        }

        playerStats.ModifyAcid(-acidCost);

        
        movementController.SetAllowRotation(true);
        _ActionManager.SetAllActionPriorityAllowed(true, 0);
        _ActionManager.StateMachine.ForceSetDefaultState();
    }



#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
