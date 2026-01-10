using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// For enemies that actively try to search for the player while in their Idle state,
// and transition to their Aggro state once they are hit or the player is spotted.

public class RoamingEnemyActionManager : EnemyActionManager
{
    [SerializeField]
    protected CharacterState _Aggro;

    [SerializeField]
    public bool isAggro;

    protected override void Awake()
    {
        base.Awake();
        
        if (isAggro) StateMachine.DefaultState = _Aggro;
    }

    public override void Hitstun()
    {
        if(!isAggro) SpottedPlayer();

        base.Hitstun();
    }

    public void SpottedPlayer()
    {
        isAggro = true;
        defaultState.StopAllCoroutines();
        StateMachine.ForceSetState(_Aggro);
        StateMachine.DefaultState = _Aggro;
        StopAllCoroutines();
        StartCoroutine(UpdateAttackStates());
    }
}
