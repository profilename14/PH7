using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using Animancer;
using Animancer.FSM;

public class PlayerMove : CharacterState, IPassPlayerDirectionalInput
{
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private Vector2 moveDir;

    [SerializeField]
    TransitionAsset moveAnimation;

    public override bool CanEnterState
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Move];

    public void PassPlayerDirectionalInput(PlayerDirectionalInput playerDirectionalInput)
    {

    }

    public void UpdateInputs(PlayerCharacterInputs input)
    {
        movementController.SetInputs(ref input);
    }

    protected override void OnEnable()
    {
        _ActionManager.anim.Play(moveAnimation);
    }

    protected void Update()
    {
        
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
