using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class UrchidSpikesAttack : AttackState
{
    [SerializeField]
    private ClipTransition spikesAttack;

    private EnemyMovementController movementController;
    private EnemyVFXManager vfx;
    public override bool CanEnterState => _ActionManager.allowedStates[this];

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren<EnemyMovementController>(ref movementController);
        vfx = (EnemyVFXManager)_Character.VFXManager;
    }

    protected override void OnEnable()
    {
        if(movementController == null) gameObject.GetComponentInParentOrChildren<EnemyMovementController>(ref movementController);

        _ActionManager.SetAllActionPriorityAllowed(false);

        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(false);
        //_Character.SetIsKnockbackImmune(true);

        AnimancerState currentState = _ActionManager.anim.Play(spikesAttack);
        currentState.Events(this).OnEnd ??= SpikesEnd;
    }

    public void SpikesStart()
    {
        // VFX
    }

    public void SpikesEnd()
    {
        //_Character.SetIsKnockbackImmune(false);
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Hitstun, true);
        _ActionManager.StateMachine.ForceSetDefaultState();
    }
}
