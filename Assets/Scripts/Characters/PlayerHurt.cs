using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer.FSM;
using Animancer;

public class PlayerHurt : TakeDamageState
{
    [SerializeField]
    ClipTransition takeDamageAnimation;

    AnimancerState currentState;

    public override bool CanEnterState
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Hitstun];

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);
        _ActionManager.SetAllStatesAllowed(false);
        Player.instance.SetIsKnockbackImmune(true);

        currentState = _ActionManager.anim.Play(takeDamageAnimation);
        currentState.Time = 0;
        currentState.Events(this).OnEnd ??= EndHitStun;
    }

    private void EndHitStun()
    {
        //Debug.Log("Ending hitstun");
        
        Player.instance.SetIsKnockbackImmune(false);
        _ActionManager.EndHitStun();
        _ActionManager.SetAllActionPriorityAllowed(true);
        _ActionManager.StateMachine.ForceSetDefaultState();
    }
}
