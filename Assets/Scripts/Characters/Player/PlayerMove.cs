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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateInputs(PlayerCharacterInputs input)
    {
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
