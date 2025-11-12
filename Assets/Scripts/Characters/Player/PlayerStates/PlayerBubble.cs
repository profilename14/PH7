using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;
using Unity.Mathematics;

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
    [SerializeReference]
    ClipTransition focusCharge;


    [SerializeField]
    GameObject bubblePrefab;

    [SerializeField]
    Vector3 spawnOffset;

    private GameObject bubbleObject;

    private AnimancerState currentState;

    PlayerDirectionalInput directionalInput;

    private bool charging = false;
    private float chargeTimer = 0;



    // Uses allowedActions to control if entering this state is allowed.
    public override bool CanEnterState 
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Low] && playerStats.alkaline >= alkalineCost;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref playerStats);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
    }

    protected override void OnEnable()
    {
        Debug.Log("Charging starte!");
        _ActionManager.SetAllActionPriorityAllowed(false);

        directionalInput = actionManager.GetDirectionalInput();

        movementController.SetAllowRotation(true);

        charging = true;
        chargeTimer = 0f;

        playerStats.ModifyAlkaline(-alkalineCost);

        Vector3 ArrowLocation = transform.position + transform.forward * 5;

        Vector3 curRotation = directionalInput.lookDir;
        float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

        bubbleObject = Instantiate(bubblePrefab, ArrowLocation + spawnOffset, Quaternion.Euler(0, angle, 0));
    }

    private void Update()
    {
        if (charging)
        {
            movementController.RotateToDir(directionalInput.lookDir);
            bubbleObject.transform.position = transform.position + transform.forward * 5 + spawnOffset;
            chargeTimer += Time.deltaTime;
        }
    }

    public void StartThrow()
    {
        currentState = _ActionManager.anim.Play(focusAnimation);
        // Just sets to idle after this animation fully ends.
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        charging = false;

        Debug.Log("Charging DONE!");
        Debug.Log(chargeTimer);

        movementController.RotateToDir(directionalInput.lookDir);
    }

    public void OnFinishCast() {

        charging = false;

        FloatingBubble bubble = bubbleObject.GetComponent<FloatingBubble>();

        if (bubble != null)
        {
            bubble.force = getBubbleForce();
            bubble.direction = transform.forward;
            bubble.SendOut();
        }
        //Debug.Log(getBubbleForce());

        chargeTimer = 0;

        movementController.SetVelocity(new Vector3(0, 15f, 0));
        
        movementController.SetAllowRotation(true);
        _ActionManager.SetAllActionPriorityAllowed(true, 0);
        _ActionManager.StateMachine.ForceSetDefaultState();
    }

    private float getBubbleForce()
    {
        //if (chargeTimer == 0) // <0.2 second
        //{
            return 90;
        //}
        /*else if (chargeTimer < 2f)
        {
            return 100 * chargeTimer; // 
        }
        else // >0.6 seconds
        {
            return 100;
        }*/
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
