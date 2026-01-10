using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer.FSM;
using Animancer;

public class TakeDamageState : CharacterState
{
    [SerializeField]
    ClipTransition flinchAnimation;

    AnimancerState currentState;

    [SerializeField]
    bool canMoveWhileStunned;

    [SerializeField]
    bool canRotateWhileStunned;

    public override bool CanEnterState
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Hitstun];

    protected override void OnEnable()
    {
        _MovementController.SetAllowMovement(canMoveWhileStunned);
        _MovementController.SetAllowRotation(canRotateWhileStunned);
        if(_MovementController is EnemyMovementController e)
        {
            e.SetForceManualRotation(false);
            e.SetForceLookAtPlayer(false);
            e.SetAIEnabled(false);
        }
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);
        _ActionManager.SetAllStatesAllowed(false);

        currentState = _ActionManager.anim.Play(flinchAnimation);
        currentState.Time = 0.1f;
        currentState.Events(this).OnEnd ??= EndHitStun;

        StartCoroutine(HitStopEnumerator(0.15f));
    }

    private void EndHitStun()
    {
        //Debug.Log("Ending hitstun");

        _MovementController.SetAllowMovement(true);
        _MovementController.SetAllowRotation(true);
        _ActionManager.EndHitStun();
        _ActionManager.SetAllActionPriorityAllowed(true);
        if (_MovementController is EnemyMovementController e)
        {
            e.SetAIEnabled(true);
        }
        _ActionManager.StateMachine.ForceSetDefaultState();
    }

    private IEnumerator HitStopEnumerator(float duration)
    {
        currentState.IsPlaying = false;
        yield return new WaitForSeconds(duration);
        currentState.IsPlaying = true;
    }
}
