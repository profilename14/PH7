using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Animancer.FSM;
using Animancer;
using System;

[System.Serializable]
public class PlayerDirectionalInput
{
    public Vector3 moveDir;
    public Vector3 lookDir;
    public bool usingController;
}

public class PlayerActionManager : CharacterActionManager
{
    [SerializeField]
    PlayerMovementController movementController;


    [Header("Player States")]
    [SerializeField]
    private PlayerJump jumpState;
    //[SerializeField]
    //private CharacterState interactState;
    [SerializeField]
    private PlayerSwordAttack attackState;
    [SerializeField]
    private DashState dashState;
    [SerializeField]
    private CharacterFocus coreState;
    [SerializeField]
    private PlayerBubble bubbleState;
    [SerializeField]
    private PlayerChargeAttack chargeAttackState;
    
    [SerializeField]
    private PlayerDashAttack dashAttackState;
    [SerializeField]
    private CharacterState spellAttackState;
    [SerializeField]
    private TakeDamageState takeDamageState;

    private PlayerMove moveState;

    private InputMaster controls;
    private PlayerInput playerInput;

    // Need InputAction ref to use ReadValue for any continuous polling
    private InputAction movementAction;
    private InputAction lookAction;

    private Vector3 moveDir;
    private Vector3 lookDir;

    private bool jumpHeld;

    private StateMachine<CharacterState>.InputBuffer inputBuffer;

    [SerializeField]
    private PlayerDirectionalInput playerDirectionalInput = new PlayerDirectionalInput();

    [SerializeField]
    private float inputTimeOut;

    [SerializeField]
    private float invincibilityTime = 1f;

    [SerializeField]
    public bool lockedOn;

    [SerializeField]
    private GameObject lockOnTarget;

    [SerializeField]
    private float lockOnRange;

    [SerializeField]
    private GameObject lockOnIcon;

    private LayerMask enemyLayerMask;
    
    public float dashTimer = 0f;
    public float pogoTimer = 0f;
    public float pogoCooldown = 0.4f;
    public bool hasDashedInAir = false;
    
    public bool hasBubbledInAir = false;

    [SerializeField]
    private Vector3 cameraAngle = new Vector3(0, 135, 0);

    public delegate void InteractDelegate();

    public InteractDelegate interactCallback;

    public bool isDashHeld = false;

    public bool DEBUG_HASDASH = false;
    public bool DEBUG_HASBUBBLE = false;

    protected override void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref attackState);
        gameObject.GetComponentInParentOrChildren(ref chargeAttackState);
        gameObject.GetComponentInParentOrChildren(ref takeDamageState);
        moveState = (PlayerMove) defaultState;
        controls = new InputMaster();
        playerInput = GetComponent<PlayerInput>();
        inputBuffer = new StateMachine<CharacterState>.InputBuffer(StateMachine);
        enemyLayerMask = LayerMask.GetMask("LockOnTarget");
        if (lockOnIcon)
        {
            lockOnIcon.GetComponent<Renderer>().material.renderQueue = 4000;
        }
    }

    private void OnEnable()
    {
        // Continuous inputs
        movementAction = controls.Typhis.Movement;
        movementAction.Enable();
        lookAction = controls.Typhis.Look;
        lookAction.Enable();

        // Discrete inputs
        controls.Typhis.Attack.Enable();
        controls.Typhis.Attack.started += context => OnAttackStarted(context);
        controls.Typhis.Attack.performed += context => OnAttackPerformed(context);

        controls.Typhis.Jump.Enable();
        controls.Typhis.Jump.started += context => OnJumpStarted(context);
        controls.Typhis.Jump.performed += context => OnJumpPerformed(context);

        if(DEBUG_HASDASH)controls.Typhis.Dash.Enable();
        else controls.Typhis.Dash.Disable();
        controls.Typhis.Dash.started += context => OnDash(context);
        controls.Typhis.Dash.canceled += context => OnDashReleased(context);

        if(DEBUG_HASBUBBLE)controls.Typhis.Bubble.Enable();
        else controls.Typhis.Bubble.Disable();
        controls.Typhis.Bubble.started += context => OnBubbleStarted(context);
        controls.Typhis.Bubble.performed += context => OnBubblePerformed(context);

        controls.Typhis.CoreMagic.Enable();
        controls.Typhis.CoreMagic.started += context => OnCoreMagicStarted(context);
        controls.Typhis.CoreMagic.performed += context => OnCoreMagicPerformed(context);

        controls.Typhis.LockOn.Enable();
        controls.Typhis.LockOn.performed += context => OnLockOnPerformed(context);

        controls.Typhis.Interact.Enable();
        controls.Typhis.Interact.performed += context => OnInteractPerformed(context);

        controls.Typhis.QuickMap.Disable();
        controls.Typhis.OpenInventory.Disable();
        controls.Typhis.Pause.Disable();
    }

    private void OnDisable()
    {
        // Continuous inputs
        movementAction = controls.Typhis.Movement;
        movementAction.Disable();
        lookAction = controls.Typhis.Look;
        lookAction.Disable();

        // Discrete inputs
        controls.Typhis.Attack.Disable();
        controls.Typhis.Attack.started -= context => OnAttackStarted(context);
        controls.Typhis.Attack.performed -= context => OnAttackPerformed(context);

        controls.Typhis.Jump.Disable();
        controls.Typhis.Jump.started -= context => OnJumpStarted(context);
        controls.Typhis.Jump.performed -= context => OnJumpPerformed(context);

        controls.Typhis.Dash.Disable();
        controls.Typhis.Dash.started -= context => OnDash(context);
        controls.Typhis.Dash.canceled -= context => OnDashReleased(context);

        controls.Typhis.Bubble.Disable();
        controls.Typhis.Bubble.started -= context => OnBubbleStarted(context);
        controls.Typhis.Bubble.performed -= context => OnBubblePerformed(context);

        controls.Typhis.CoreMagic.Disable();
        controls.Typhis.CoreMagic.started -= context => OnCoreMagicStarted(context);
        controls.Typhis.CoreMagic.performed -= context => OnCoreMagicPerformed(context);

        controls.Typhis.Interact.Disable();
        controls.Typhis.Interact.performed -= context => OnInteractPerformed(context);

        controls.Typhis.LockOn.Disable();
        controls.Typhis.LockOn.performed -= context => OnLockOnPerformed(context);
    }

    private void Start()
    {
        if (GameManager.instance.dashUnlocked)
        {
            UnlockDash();
        }
        if (GameManager.instance.bubbleUnlocked)
        {
            UnlockBubble();
        }
    }

    private void Update()
    {
        moveDir = playerDirectionalInput.moveDir;
        lookDir = playerDirectionalInput.lookDir;

        inputBuffer.Update();

        dashTimer -= Time.deltaTime;
        pogoTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        playerDirectionalInput.usingController = false || (playerInput.currentControlScheme.Equals("Switch Pro Controller"));

        Vector2 moveInput = Vector2.ClampMagnitude(movementAction.ReadValue<Vector2>(), 1f);

        // Read movement input
        playerDirectionalInput.moveDir = new Vector3(moveInput.x, 0, moveInput.y);

        if (lockedOn)
        {
            lockOnIcon.SetActive(true);
            lockOnIcon.transform.position = lockOnTarget.transform.position;
            lockOnIcon.transform.LookAt(Camera.main.transform);
            Vector3 lockOnDirHorizontal = Quaternion.Inverse(movementController.GetCameraPlanarRotation()) * (lockOnTarget.transform.position - character.transform.position).normalized;
            playerDirectionalInput.lookDir = new Vector3(lockOnDirHorizontal.x, 0, lockOnDirHorizontal.z);
        }
        else
        {
            if (lockOnIcon)
            {
                lockOnIcon.SetActive(false);
            }
            
            if (playerDirectionalInput.usingController)
            {
                // If the right stick has input, then it should override the left stick for determining look direction.
                // Otherwise, controller usually uses left stick for lookDir.
                Vector2 rightStick = lookAction.ReadValue<Vector2>().normalized;
                if (rightStick != Vector2.zero)
                {
                    playerDirectionalInput.lookDir = GetDirRelativeToCamera(new Vector3(rightStick.x, 0, rightStick.y));
                }
                else if (moveDir != Vector3.zero) playerDirectionalInput.lookDir = (Quaternion.Euler(0f, -45f, 0f) * moveDir).normalized;
            }
            else playerDirectionalInput.lookDir = GetMouseDirection();
        }

        movementController.ProcessMoveInput(playerDirectionalInput.moveDir);

        if (movementController.IsGrounded())
        {
            hasDashedInAir = false;
            hasBubbledInAir = false;
        }
    }

    //
    // INPUT ACTION METHODS
    //

    void OnJumpStarted(InputAction.CallbackContext context)
    {
        hasDashedInAir = false;
        hasBubbledInAir = false;
        // Jump button is held
        jumpHeld = true;
        if (!StateMachine.TrySetState(jumpState)) inputBuffer.Buffer(jumpState, inputTimeOut);
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Jump button is released
        jumpHeld = false;
    }

    void OnAttackStarted(InputAction.CallbackContext context)
    {
        // If the button is held for at least 0.5s
        if (context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            // If it fails to enter the charge attack state, buffer it.
            if (!StateMachine.TrySetState(chargeAttackState)) inputBuffer.Buffer(chargeAttackState, inputTimeOut);
        }
    }

    void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // If the button is released within 0.5s
        if(context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
        {
            // If it fails to enter the SwordAttack state, buffer it.
            if (false && isDashHeld == true)
            {
                if (!hasDashedInAir)
                {
                    if (!StateMachine.TrySetState(dashAttackState)) inputBuffer.Buffer(dashAttackState, inputTimeOut);
                }
                
                
            }
            else if (!StateMachine.TryResetState(attackState))
            {
                /*if (inputBuffer.State != null && 
                    (inputBuffer.State.StateName == "PlayerDash" || inputBuffer.State.StateName == "PlayerDashAttack"))
                {
                    print("dashAttacked");
                    inputBuffer.Buffer(dashAttackState, inputTimeOut);
                }
                else {
                }*/


                inputBuffer.Buffer(attackState, inputTimeOut);
                
                
            }
        }

        // If the button is released after 0.5s [Note: a button released when the attack has not been fully charged will cancel it]
        if (context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            if (StateMachine.CurrentState == chargeAttackState) chargeAttackState.ReleaseChargeAttack();
        }
    }

    void OnDashReleased(InputAction.CallbackContext context)
    {
        isDashHeld = false;
        StateMachine.DefaultState = moveState;
        movementController.SetSprinting(false);


    }

    void OnDash(InputAction.CallbackContext context)
    {
        isDashHeld = true;

        if (StateMachine.CurrentState.StateName == "PlayerDash")
        {
            dashState.dashButtonHit();
        }


        if ((inputBuffer.State != null &&
                 inputBuffer.State.StateName == "PlayerSwordAttack") || StateMachine.CurrentState.StateName == "PlayerSwordAttack")
            {
                /*print("dashAttacked");
                if (StateMachine.CurrentState.StateName == "PlayerSwordAttack")
                {
                    inputBuffer.Buffer(dashAttackState, inputTimeOut);
                }
                else
                {
                    inputBuffer.Buffer(dashAttackState, inputTimeOut);
                    print(inputBuffer.State.StateName);
                }
                return;*/
            }

        if (dashTimer > 0 || hasDashedInAir)
        {
            return;
        }
        else if (playerDirectionalInput.moveDir == Vector3.zero)
        {
            movementController.SetSprinting(true);
            return;
        }
        hasDashedInAir = true;
        //dashThisFrame = true;

        if (!StateMachine.TryResetState(dashState))
        {
            inputBuffer.Buffer(dashState, inputTimeOut);
            StateMachine.TrySetState(dashState);

        }
        
        character.SetIsInvincible(true);
        character.SetIsKnockbackImmune(true);
        character.SetIsHitstunImmune(true);
    }

    void OnBubbleStarted(InputAction.CallbackContext context)
    {

        if(context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            // If it fails to enter the charge attack state, buffer it.
            if (!StateMachine.TrySetState(bubbleState)) inputBuffer.Buffer(bubbleState, inputTimeOut);
        }
    }

    void OnBubblePerformed(InputAction.CallbackContext context)
    {
        if (hasBubbledInAir)
        {
            return;
        }
        hasBubbledInAir = true;

        // If the button is released within 0.2s
        if (context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
        {
            // If it fails to enter the SwordAttack state, buffer it.
            //if (!StateMachine.TryResetState(bubbleState)) inputBuffer.Buffer(bubbleState, inputTimeOut);
            // If the above fails we must have no resources. Default to bubble
            StateMachine.TrySetState(bubbleState);

            if (StateMachine.CurrentState == bubbleState) bubbleState.StartThrow();
        }

        // If the button is released after 0.2s
        if (context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            if (StateMachine.CurrentState == bubbleState) bubbleState.StartThrow();
        }
    }

    void OnCoreMagicStarted(InputAction.CallbackContext context)
    {
        if(context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            // If it fails to enter the charge attack state, buffer it.
            if (!StateMachine.TrySetState(coreState)) inputBuffer.Buffer(coreState, inputTimeOut);
        }
    }

    void OnCoreMagicPerformed(InputAction.CallbackContext context)
    {
        // If the button is released within 0.5s
        if (context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
        {
            // If it fails to enter the SwordAttack state, buffer it.
            if (!StateMachine.TryResetState(spellAttackState)) inputBuffer.Buffer(spellAttackState, inputTimeOut);
            // If the above fails we must have no resources. Default to bubble
            //StateMachine.TrySetState(bubbleState);
        }

        // If the button is released after 0.5s [Note: a button released when the attack has not been fully charged will cancel it]
        if (context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            if (StateMachine.CurrentState == coreState) coreState.ReleaseFocus();
        }
    }

    void OnInteractPerformed(InputAction.CallbackContext context)
    {
        //Debug.Log("Interact");
        interactCallback?.Invoke();
    }

    void OnLockOnPerformed(InputAction.CallbackContext context)
    {
        if (lockedOn)
        {
            lockedOn = false;
            lockOnTarget.GetComponentInParentOrChildren<Enemy>().DisableLockOn();
            return;
        }

        Collider[] enemyColliders = Physics.OverlapSphere(Player.instance.transform.position, lockOnRange, enemyLayerMask);

        if (enemyColliders.Length == 0) return;

        GameObject closestEnemy = enemyColliders[0].gameObject;

        float closestEnemyDistance = lockOnRange + 1;

        foreach (Collider c in enemyColliders)
        {
            float dist = Vector3.Distance(Player.instance.transform.position, c.gameObject.transform.position);
            if (dist < closestEnemyDistance)
            {
                closestEnemy = c.gameObject;
                closestEnemyDistance = dist;
            }
        }

        lockOnTarget = closestEnemy;
        lockOnTarget.GetComponentInParentOrChildren<Enemy>().LockOn(OnLockOnTargetDie);
        lockedOn = true;
    }

    void OnLockOnTargetDie()
    {
        lockedOn = false;
        lockOnTarget = null;
    }

    //
    // STATE OVERRIDE METHODS
    //

    public override void Hitstun()
    {
        playerDirectionalInput.moveDir = Vector3.zero;
        movementController.ProcessMoveInput(playerDirectionalInput.moveDir);
        StateMachine.ForceSetState(takeDamageState);
        character.SetIsInvincible(true);
    }

    public void ForceDashAttackState()
    {
        inputBuffer.Clear();

        if (dashTimer <= 0 && !hasDashedInAir)
        {
            hasDashedInAir = true;
            StateMachine.ForceSetState(dashAttackState);
        }
        else {
            /*movementController.SetAllowMovement(true);
            movementController.SetAllowRotation(true);
            SetAllActionPriorityAllowed(true);
            StateMachine.ForceSetDefaultState();*/
        }
    }

    public override void EndHitStun()
    {
        StartCoroutine(EndInvincibility());
    }

    public IEnumerator EndInvincibility()
    {
        yield return new WaitForSeconds(invincibilityTime);
        character.SetIsInvincible(false);
        //Debug.Log("Is end invincibility");
    }

    public void EndDash(float dashCooldown)
    {
        dashTimer = dashCooldown;
        character.SetIsHitstunImmune(false);
        character.SetIsKnockbackImmune(false);
        //Debug.Log("Is end dash");
        character.SetIsInvincible(false);

        if (isDashHeld)
        {
            movementController.SetSprinting(true);
        }
    }

    public void PogoCooldown()
    {
        pogoTimer = pogoCooldown;
    }

    public void RefreshDash()
    {
        hasDashedInAir = false;
    }

    //
    // INPUT HELPER METHODS
    //

    public bool IsJumpHeld()
    {
        return jumpHeld;
    }

    public PlayerDirectionalInput GetDirectionalInput()
    {
        return playerDirectionalInput;
    }

    public Vector3 GetMouseDirection()
    {
        Vector3 dir;
        float h = Mouse.current.position.ReadValue().x - Screen.width / 2;
        float v = Mouse.current.position.ReadValue().y - Screen.height / 2;
        dir = new Vector3(h, 0, v);
        dir.Normalize();

        dir = GetDirRelativeToCamera(dir);
        return dir;
    }

    public void UnlockDash()
    {
        DEBUG_HASDASH = true;
        controls.Typhis.Dash.Enable();
    }

    public void UnlockBubble()
    {
        DEBUG_HASBUBBLE = true;
        controls.Typhis.Bubble.Enable();
    }

    // Old function, not always relative unless you multiply by movementController.GetCameraPlanarRotation()
    public Vector3 GetDirRelativeToCamera(Vector3 dir)
    {
        return Quaternion.Euler(cameraAngle) * dir;
    }

    public CharacterState GetBufferedState()
    {
        return inputBuffer.State;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
