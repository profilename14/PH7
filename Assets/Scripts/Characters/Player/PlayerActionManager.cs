using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Animancer.FSM;
using Animancer;

public class PlayerActionManager : CharacterActionManager
{
    [Header("Player States")]
    [SerializeField]
    private PlayerMove _Move;
    [SerializeField]
    private CharacterState _Interact;
    [SerializeField]
    private PlayerSwordAttack _SwordAttack;
    [SerializeField]
    private CharacterState _Dash;
    [SerializeField]
    private CharacterFocus _Core;
    [SerializeField]
    private CharacterState _Bubble;
    [SerializeField]
    private PlayerChargeAttack _ChargeAttack;
    [SerializeField]
    private CharacterState _SpellAttack;

    private InputMaster controls;

    // Need InputAction ref to use ReadValue for any continuous polling
    private InputAction _MovementAction;

    private Vector2 moveDir;

    private bool jumpPressed;
    private bool jumpHeld;
    private bool dashThisFrame;

    private StateMachine<CharacterState>.InputBuffer _InputBuffer;

    [SerializeField]
    private float inputTimeOut;

    protected override void Awake()
    {
        base.Awake();
        controls = new InputMaster();
        //_AllowedActions.Add(_Dash, true);
        _InputBuffer = new StateMachine<CharacterState>.InputBuffer(StateMachine);
    }

    private void OnEnable()
    {
        _MovementAction = controls.Typhis.Movement;
        _MovementAction.Enable();
        controls.Typhis.Attack.Enable();
        controls.Typhis.Attack.performed += context => OnAttackPerformed(context);
        controls.Typhis.Attack.started += context => OnAttackStarted(context);
        controls.Typhis.Jump.Enable();
        controls.Typhis.Jump.started += context => { jumpPressed = true;  jumpHeld = true; };
        controls.Typhis.Jump.performed += context => { jumpHeld = false; };
        controls.Typhis.Jump.canceled += context => { jumpHeld = false; };
        controls.Typhis.Dash.Enable();
        controls.Typhis.Dash.performed += context => OnDash(context);
        controls.Typhis.Bubble.Enable();
        controls.Typhis.Bubble.performed += context => OnBubble(context);
        controls.Typhis.CoreMagic.Enable();
        controls.Typhis.CoreMagic.performed += context => OnCoreMagicPerformed(context);
        controls.Typhis.CoreMagic.started += context => OnCoreMagicStarted(context);
    }

    private void OnDisable()
    {
        // Fill in disabling
    }

    private void Update()
    {
        PassInput();

        // As long as the enter/exit variables on the states are set correctly this should cause no problems
        if(moveDir != Vector2.zero || jumpHeld)
        {
            StateMachine.TrySetState(_Move);
        }
        else
        {
            if(StateMachine.CurrentState == _Move) StateMachine.TrySetDefaultState();
        }

        _InputBuffer.Update();
    }

    private void FixedUpdate()
    {
        // Read movement input
        moveDir = _MovementAction.ReadValue<Vector2>();
    }

    // This function is used to pass any sort of input to the currently active state.
    // This should be done here and not in the state, as all Input event handling is in this script.
    private void PassInput()
    {
        if(StateMachine.CurrentState == _Move)
        {
            PlayerCharacterInputs input = new PlayerCharacterInputs();
            input.MoveAxisForward = moveDir.y;
            input.MoveAxisRight = moveDir.x;
            input.jumpPressed = jumpPressed;
            input.JumpHeld = jumpHeld;
            input.Dash = dashThisFrame;
            _Move.UpdateInputs(input);
            jumpPressed = false;
            dashThisFrame = false;
        }
        else if(StateMachine.CurrentState == _SwordAttack)
        {
            PlayerCharacterInputs input = new PlayerCharacterInputs();
            input.MoveAxisForward = moveDir.y;
            input.MoveAxisRight = moveDir.x;
            _SwordAttack.UpdateInputs(input);
        }
    }

    // Receives an attack action performed
    void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // If the button is released within 0.5s
        if(context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
        {
            // If it fails to enter the SwordAttack state, buffer it.
            if (!StateMachine.TryResetState(_SwordAttack)) _InputBuffer.Buffer(_SwordAttack, inputTimeOut);
        }

        // If the button is released after 0.5s [Note: a button released when the attack has not been fully charged will cancel it]
        if (context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            if (StateMachine.CurrentState == _ChargeAttack) _ChargeAttack.ReleaseChargeAttack();
        }
    }

    // Receives an attack action started
    void OnAttackStarted(InputAction.CallbackContext context)
    {
        // If the button is held for at least 0.5s
        if(context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            // If it fails to enter the charge attack state, buffer it.
            if (!StateMachine.TrySetState(_ChargeAttack)) _InputBuffer.Buffer(_ChargeAttack, inputTimeOut);
        }
    }

    // Receives a dash button press
    void OnDash(InputAction.CallbackContext context)
    {
        dashThisFrame = true;
    }

    void OnBubble(InputAction.CallbackContext context)
    {
        StateMachine.TrySetState(_Bubble);
    }

    void OnCoreMagicPerformed(InputAction.CallbackContext context)
    {
        // If the button is released within 0.5s
        if(context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
        {
            // If it fails to enter the SwordAttack state, buffer it.
            if (!StateMachine.TryResetState(_SpellAttack)) _InputBuffer.Buffer(_SpellAttack, inputTimeOut);
            StateMachine.TrySetState(_Bubble);
        }

        // If the button is released after 0.5s [Note: a button released when the attack has not been fully charged will cancel it]
        if (context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            if (StateMachine.CurrentState == _Core) _Core.ReleaseFocus();
        }
    }

    void OnCoreMagicStarted(InputAction.CallbackContext context)
    {
        if(context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            // If it fails to enter the charge attack state, buffer it.
            if (!StateMachine.TrySetState(_Core)) _InputBuffer.Buffer(_Core, inputTimeOut);
        }
    }



    public override void Hitstun()
    {
        return;
    }

    public override void EndHitStun()
    {
        return;
    }
}
