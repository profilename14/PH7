using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public abstract class CharacterState : StateBehaviour
{
    [SerializeField]
    protected Character _Character;
    public Character character => _Character;

    protected CharacterActionManager _ActionManager;

    protected CharacterMovementController _MovementController;

    protected static readonly StringReference AllowHitstunEvent = "AllowHitstun";
    protected static readonly StringReference AllowHighPriorityEvent = "AllowHighPriority";
    protected static readonly StringReference AllowMediumPriorityEvent = "AllowMediumPriority";
    protected static readonly StringReference AllowLowPriorityEvent = "AllowLowPriority";
    protected static readonly StringReference AllowMoveStateEvent = "AllowMoveState";
    protected static readonly StringReference AllowJumpStateEvent = "AllowJumpState";
    protected static readonly StringReference AllowMovementEvent = "AllowMovement";
    protected static readonly StringReference AllowRotationEvent = "AllowRotation";

    protected void Awake()
    {
        gameObject.GetComponentInParentOrChildren(ref _Character);
        _ActionManager = _Character.actionManager;
        _MovementController = _Character.movementController;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

    }
#endif

    protected virtual void OnEnable()
    {
        return;
    }

    protected virtual void OnDisable()
    {
        return;
    }

    public void AllowHitstun()
    {
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Hitstun, true);
    }

    public void AllowHighPriority()
    {
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.High, true);
    }

    public void AllowMediumPriority()
    {
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Medium, true);
    }

    public void AllowLowPriority()
    {
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Low, true);
    }

    public void AllowMove()
    {
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Move, true);
    }

    public void AllowJump()
    {
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Jump, true);
    }

    /*public void AllowMovement(bool isAllowed)
    {
        _MovementController.SetAllowMovement(isAllowed);
    }

    public void AllowRotation(bool isAllowed)
    {
        _MovementController.SetAllowRotation(isAllowed);
    }*/
}
