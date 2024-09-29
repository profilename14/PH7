using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer.FSM;
using Animancer;

public class TakeDamageState : CharacterState
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

        currentState = _ActionManager.anim.Play(takeDamageAnimation);
        currentState.Time = 0;
        currentState.Events(this).OnEnd ??= EndHitStun;
    }

    private void EndHitStun()
    {
        //Debug.Log("Ending hitstun");
        _ActionManager.EndHitStun();
        _ActionManager.StateMachine.ForceSetDefaultState();
    }
}
