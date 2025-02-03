using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Animancer.FSM;
using Animancer;

[System.Serializable]
public class PlayerDirectionalInput
{
    public Vector3 moveDir;
    public Vector3 lookDir;
    public bool usingController;
}

public class PlayerActionManager : CharacterActionManager
{
    [Header("Player States")]
    [SerializeField]
    private PlayerJump jumpState;
    [SerializeField]
    private CharacterState interactState;
    [SerializeField]
    private PlayerSwordAttack attackState;
    [SerializeField]
    private CharacterState dashState;
    [SerializeField]
    private CharacterFocus coreState;
    [SerializeField]
    private CharacterState bubbleState;
    [SerializeField]
    private PlayerChargeAttack chargeAttackState;
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

    private bool usingController;

    [SerializeField]
    private PlayerDirectionalInput playerDirectionalInput = new PlayerDirectionalInput();

    [SerializeField]
    private float inputTimeOut;

    [SerializeField]
    private float invincibilityTime = 1f;

    [SerializeField]
    private Vector3 cameraAngle = new Vector3(0, 135, 0);

    protected override void Awake()
    {
        base.Awake();
        moveState = (PlayerMove) defaultState;
        controls = new InputMaster();
        playerInput = GetComponent<PlayerInput>();
        inputBuffer = new StateMachine<CharacterState>.InputBuffer(StateMachine);
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

        controls.Typhis.Dash.Enable();
        controls.Typhis.Dash.performed += context => OnDash(context);

        controls.Typhis.Bubble.Enable();
        controls.Typhis.Bubble.performed += context => OnBubble(context);

        controls.Typhis.CoreMagic.Enable();
        controls.Typhis.CoreMagic.started += context => OnCoreMagicStarted(context);
        controls.Typhis.CoreMagic.performed += context => OnCoreMagicPerformed(context);
    }

    private void OnDisable()
    {
        // Fill in disabling at some point
    }

    private void Update()
    {
        moveDir = playerDirectionalInput.moveDir;
        lookDir = playerDirectionalInput.lookDir;

        inputBuffer.Update();
    }

    private void FixedUpdate()
    {
        usingController = (playerInput.currentControlScheme == "Controller");

        Vector2 moveInput = Vector2.ClampMagnitude(movementAction.ReadValue<Vector2>(), 1f);

        // Read movement input
        playerDirectionalInput.moveDir = new Vector3(moveInput.x, 0, moveInput.y);

        if (usingController)
        {
            // If the right stick has input, then it should override the left stick for determining look direction.
            // Otherwise, controller usually uses left stick for lookDir.
            Vector2 rightStick = lookAction.ReadValue<Vector2>().normalized;
            if (rightStick != Vector2.zero) playerDirectionalInput.lookDir = GetDirRelativeToCamera(new Vector3(rightStick.x, 0, rightStick.y));
            else playerDirectionalInput.lookDir = GetDirRelativeToCamera(moveDir);
        }
        else playerDirectionalInput.lookDir = GetMouseDirection();
    }

    //
    // INPUT ACTION METHODS
    //

    void OnJumpStarted(InputAction.CallbackContext context)
    {
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
            if (!StateMachine.TryResetState(attackState)) inputBuffer.Buffer(attackState, inputTimeOut);
        }

        // If the button is released after 0.5s [Note: a button released when the attack has not been fully charged will cancel it]
        if (context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            if (StateMachine.CurrentState == chargeAttackState) chargeAttackState.ReleaseChargeAttack();
        }
    }

    void OnDash(InputAction.CallbackContext context)
    {
        //dashThisFrame = true;
        if (!StateMachine.TryResetState(dashState)) inputBuffer.Buffer(dashState, inputTimeOut);
        StateMachine.TrySetState(dashState);
    }

    void OnBubble(InputAction.CallbackContext context)
    {
        if (!StateMachine.TryResetState(bubbleState)) inputBuffer.Buffer(bubbleState, inputTimeOut);
        StateMachine.TrySetState(bubbleState);
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
            StateMachine.TrySetState(bubbleState);
        }

        // If the button is released after 0.5s [Note: a button released when the attack has not been fully charged will cancel it]
        if (context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            if (StateMachine.CurrentState == coreState) coreState.ReleaseFocus();
        }
    }

    //
    // STATE OVERRIDE METHODS
    //

    public override void Hitstun()
    {
        StateMachine.ForceSetState(takeDamageState);
        character.SetIsInvincible(true);
    }

    public override void EndHitStun()
    {
        StartCoroutine(EndInvincibility());
    }

    public IEnumerator EndInvincibility()
    {
        yield return new WaitForSeconds(invincibilityTime);
        character.SetIsInvincible(false);
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
        float h = Input.mousePosition.x - Screen.width / 2;
        float v = Input.mousePosition.y - Screen.height / 2;
        dir = new Vector3(h, 0, v);
        dir.Normalize();

        dir = GetDirRelativeToCamera(dir);
        return dir;
    }

    public Vector3 GetDirRelativeToCamera(Vector3 dir)
    {
        return Quaternion.Euler(cameraAngle) * dir;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref attackState);
        gameObject.GetComponentInParentOrChildren(ref chargeAttackState);
        gameObject.GetComponentInParentOrChildren(ref takeDamageState);
    }
#endif
}
