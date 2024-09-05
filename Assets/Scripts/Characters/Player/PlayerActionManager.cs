using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionManager : CharacterActionManager
{
    [Header("Player States")]
    [SerializeField]
    private PlayerMove _Move;
    [SerializeField]
    private CharacterState _Interact;
    [SerializeField]
    private AttackState _SwordAttack;
    [SerializeField]
    private CharacterState _Dash;
    [SerializeField]
    private CharacterState _Core;
    [SerializeField]
    private CharacterState _Bubble;
    [SerializeField]
    private CharacterState _ChargeAttack;
    [SerializeField]
    private CharacterState _SpellAttack;

    private InputMaster controls;

    private InputAction _MovementAction;

    private Vector2 moveDir;

    protected override void Awake()
    {
        base.Awake();
        controls = new InputMaster();
    }

    private void OnEnable()
    {
        _MovementAction = controls.Typhis.Movement;
        _MovementAction.Enable();
        controls.Typhis.Attack.Enable();
        controls.Typhis.Attack.performed += context => OnAttack(context);
    }

    private void OnDisable()
    {
        // Fill in disabling
    }

    private void Update()
    {
        PassInput();

        if(moveDir != Vector2.zero)
        {
            StateMachine.TrySetState(_Move);
        }
        else
        {
            StateMachine.TrySetDefaultState();
        }
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
            _Move.UpdateMovement(moveDir);
        }
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        if(context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
        {
            Debug.Log("Attack [short press]");
        }

        if (context.interaction is UnityEngine.InputSystem.Interactions.SlowTapInteraction)
        {
            Debug.Log("Attack [long press]");
        }
    }
}
