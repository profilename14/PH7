using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class StriderDash : AttackState
{
    [SerializeField]
    private ClipTransition dashAttack;

    [SerializeField]
    private float dashForce;

    private EnemyActionManager enemyActionManager;

    public override bool CanEnterState => actionManager.allowedActions[this];

    private void Awake()
    {
        enemyActionManager = (EnemyActionManager)actionManager;
    }

    protected override void OnEnable()
    {
        enemyActionManager.SetAllActionsAllowed(false);

        enemyActionManager.pathfinding.isStopped = true;
        enemyActionManager.pathfinding.enableRotation = false;
        // Should try allowing rotation until start of dash

        enemyActionManager.target.transform.position = Player.instance.transform.position;

        AnimancerState currentState = enemyActionManager.anim.Play(dashAttack);
        currentState.Events(this).OnEnd ??= actionManager.StateMachine.ForceSetDefaultState;
    }

    public void DashStart()
    {
        //Debug.Log("Dash start");
        Rigidbody rb = character.gameObject.GetComponent<Rigidbody>();
        rb.AddForce(character.transform.forward * dashForce, ForceMode.Impulse);
    }
}
