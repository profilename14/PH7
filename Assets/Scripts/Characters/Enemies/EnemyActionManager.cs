using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Animancer;
using Animancer.FSM;

[System.Serializable]
public struct EnemyAttack
{
    public float cooldown;
    //public CharacterActionPriority priority;
    public CharacterState stateScript;
    public EnemyAttackBehaviorData behaviorData;
}

public class EnemyActionManager : CharacterActionManager
{
    [SerializeField]
    protected CharacterState _InitAggro;

    [SerializeField]
    protected CharacterState _HitStun;

    [SerializeField]
    // How often (in seconds) should this Enemy update attack behavior?
    protected float attackBehaviorUpdateInterval = 0.1f;

    [SerializeField]
    // Flag for enemy types such as the Urchid.
    // Never goes into aggro state, always stays in patrol state.
    protected bool nonFollowingEnemy;

    [SerializeField]
    // Flag for enemy types such as the Urchid.
    // Enemy can attack in patrol state.
    protected bool canAttackInPatrolState;

    [SerializeField]
    protected List<EnemyAttack> attacks = new();

    [SerializeField]
    protected float noAttacksTimer;

    [SerializeField]
    protected bool isAggro;

    // For storing a reference to the Player singleton instance.
    protected Player player;

    private List<EnemyAttack> attackCandidates = new();

    [SerializeField]
    private bool isStunned = false;

    protected override void Awake()
    {
        base.Awake();

        if (isAggro) StateMachine.DefaultState = _InitAggro;

        player = Player.instance;
                
        // Set up everything for all the attack behaviors.
        for(int i = 0; i < attacks.Count; i++)
        {
            if (attacks[i].behaviorData.startWithMaxCooldown)
            {
                _AllowedStates.Add(attacks[i].stateScript, false);
                EnemyAttack attackTemp = attacks[i];
                attackTemp.cooldown = attacks[i].behaviorData.cooldown;
                attacks[i] = attackTemp;
            }
            else
            {
                _AllowedStates.Add(attacks[i].stateScript, true);
            }
        }
    }

    public IEnumerator UpdateAttackStates()
    {
        Vector3 vectorToPlayer = Player.instance.transform.position - _Character.transform.position;

        float distanceToPlayer = vectorToPlayer.magnitude;
        float angleToPlayerForward = Vector3.Angle(_Character.transform.forward, vectorToPlayer);
        float angleToPlayerUp = Vector3.Angle(_Character.transform.up, vectorToPlayer);
        float angleToPlayerRight = Vector3.Angle(_Character.transform.right, vectorToPlayer);

        float totalFrequencies = 0;

        attackCandidates.Clear();

        for (int i = 0; i < attacks.Count; i++)
        {
            EnemyAttackBehaviorData attackData = attacks[i].behaviorData;
            
            if (attackData.decrementCooldownOnlyWhenAllowed && !_AllowedStates[attacks[i].stateScript]) continue;

            if (distanceToPlayer > attackData.distance.min && distanceToPlayer < attackData.distance.max
                && angleToPlayerForward > attackData.forwardAngle.min && angleToPlayerForward < attackData.forwardAngle.max
                && angleToPlayerUp > attackData.upAngle.min && angleToPlayerUp < attackData.upAngle.max
                && angleToPlayerRight > attackData.rightAngle.min && angleToPlayerRight < attackData.rightAngle.max)
            {
                // Within range of the attack.
                if(attacks[i].cooldown <= 0 && _AllowedStates[attacks[i].stateScript])
                {
                    // This is a possible candidate for an attack.
                    attackCandidates.Add(attacks[i]);
                    totalFrequencies += attackData.frequency;
                    continue;
                }
            }
            else
            {
                // Out of range of the attack.
                if (attackData.decrementCooldownOnlyInRange) continue;
            }


            // Handle decrementing cooldowns.
            EnemyAttack attackTemp = attacks[i];
            attackTemp.cooldown -= attackBehaviorUpdateInterval;
            attacks[i] = attackTemp;

            if (!isStunned && attacks[i].cooldown <= 0) _AllowedStates[attacks[i].stateScript] = true;
            else _AllowedStates[attacks[i].stateScript] = false;
        }

        if (!isStunned)
        {
            float randomNum = Random.Range(0, totalFrequencies);

            for (int i = 0; i < attackCandidates.Count; i++)
            {
                if (randomNum <= attackCandidates[i].behaviorData.frequency)
                {
                    if (StateMachine.TrySetState(attackCandidates[i].stateScript)) ResetCooldown(attackCandidates[i]);
                    Debug.Log("Attempting attack: " + attackCandidates[i].stateScript);
                    break;
                }

                randomNum -= attackCandidates[i].behaviorData.frequency;
            }
        }

        yield return new WaitForSeconds(attackBehaviorUpdateInterval);

        StartCoroutine(UpdateAttackStates());
    }

    public void ResetCooldown(EnemyAttack behavior)
    {
        for(int i = 0; i < attacks.Count; i++)
        {
            if(attacks[i].Equals(behavior))
            {
                EnemyAttack attackTemp = attacks[i];
                attackTemp.cooldown = attacks[i].behaviorData.cooldown;
                attacks[i] = attackTemp;
            }
        }
    }

    public void SpottedPlayer()
    {
        isAggro = true;
        _Idle.StopAllCoroutines();
        StateMachine.ForceSetState(_InitAggro);
        StateMachine.DefaultState = _InitAggro;
        StopAllCoroutines();
        StartCoroutine(UpdateAttackStates());
    }

    public override void Hitstun()
    {
        if (!isAggro && !nonFollowingEnemy) SpottedPlayer();
        if (StateMachine.TryResetState(_HitStun))
        {
            isStunned = true;
        }
    }

    public override void EndHitStun()
    {
        isStunned = false;
        Debug.Log("End stun");
    }

#if UNITY_EDITOR
    new void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
