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
    private CharacterState _Core;
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

    private bool jumpThisFrame;
    private bool dashThisFrame;

    private StateMachine<CharacterState>.InputBuffer _InputBuffer;

    [SerializeField]
    private float inputTimeOut;

    protected override void Awake()
    {
        base.Awake();
        controls = new InputMaster();
        _AllowedActions.Add(_Move, true);
        _AllowedActions.Add(_SwordAttack, true);
        _AllowedActions.Add(_ChargeAttack, true);
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
        controls.Typhis.Jump.performed += context => OnJump(context);
        controls.Typhis.Dash.Enable();
        controls.Typhis.Dash.performed += context => OnDash(context);
    }

    private void OnDisable()
    {
        // Fill in disabling
    }

    private void Update()
    {
        PassInput();

        // As long as the enter/exit variables on the states are set correctly this should cause no problems
        if(moveDir != Vector2.zero || jumpThisFrame)
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
            input.JumpDown = jumpThisFrame;
            input.Dash = dashThisFrame;
            _Move.UpdateInputs(input);
            
            jumpThisFrame = false;
            dashThisFrame = false;
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

    // Receives a jump button press
    void OnJump(InputAction.CallbackContext context)
    {
        jumpThisFrame = true;
    }

    // Receives a dash button press
    void OnDash(InputAction.CallbackContext context)
    {
        dashThisFrame = true;
    }
}
