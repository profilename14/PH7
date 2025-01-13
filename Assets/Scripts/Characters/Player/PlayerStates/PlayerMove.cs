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
    TransitionAsset moveAnimation;

    private PlayerActionManager actionManager;

    private PlayerDirectionalInput directionalInput;

    public override bool CanEnterState
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Move];

    protected override void OnEnable()
    {
        _ActionManager.anim.Play(moveAnimation);
    }

    protected void Update()
    {
        directionalInput = actionManager.GetDirectionalInput();
        movementController.RotateToDir(actionManager.GetDirRelativeToCamera(directionalInput.moveDir));
        movementController.ProcessMoveInput(directionalInput.moveDir);
        //Debug.Log("running move code: " + directionalInput.moveDir);
    }

    protected override void OnDisable()
    {
        
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
    }
#endif
}
