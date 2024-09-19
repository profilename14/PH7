using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using Animancer;
using Animancer.FSM;

public class PlayerMove : CharacterState
{
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private Vector2 moveDir;

    public override bool CanEnterState
        => actionManager.allowedActions[this];

    public void UpdateInputs(PlayerCharacterInputs input)
    {
        movementController.SetInputs(ref input);
    }

    protected override void OnDisable()
    {
        PlayerCharacterInputs input = new();
        movementController.SetInputs(ref input);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }
#endif
}
