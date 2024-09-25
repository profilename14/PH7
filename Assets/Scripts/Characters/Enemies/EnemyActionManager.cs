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
    public CharacterState stateScript;
    public EnemyAttackBehaviorData behaviorData;
}

public class EnemyActionManager : CharacterActionManager
{
    [SerializeField]
    protected CharacterState _InitAggro;

    [SerializeField]
    protected RichAI _Pathfinding;
    public RichAI pathfinding => _Pathfinding;

    [SerializeField]
    // How often (in seconds) should this Enemy update attack behavior?
    protected float attackBehaviorUpdateInterval = 0.25f;

    [SerializeField]
    protected List<EnemyAttack> attacks = new();

    [SerializeField]
    protected float noAttacksTimer;

    // For storing a reference to the Player singleton instance.
    protected Player player;

    // GameObject created at runtime to visualize the Enemy's movement target.
    public GameObject target;


    private List<EnemyAttack> attackCandidates = new();

    protected override void Awake()
    {
        // This needs to come first because the base.Awake() references these fields.
        player = Player.instance;
        target = new GameObject(_Character.gameObject.name + " AI Target");
        
        base.Awake();
        
        pathfinding.maxSpeed = _Character.characterData.baseSpeed;
        
        // Set up everything for all the attack behaviors.
        for(int i = 0; i < attacks.Count; i++)
        {
            if (attacks[i].behaviorData.startWithMaxCooldown)
            {
                _AllowedActions.Add(attacks[i].stateScript, false);
                EnemyAttack attackTemp = attacks[i];
                attackTemp.cooldown = attacks[i].behaviorData.cooldown;
                attacks[i] = attackTemp;
            }
            else
            {
                _AllowedActions.Add(attacks[i].stateScript, true);
            }
        }
    }

    protected virtual void Update()
    {
        _Pathfinding.destination = target.transform.position;
    }

    public IEnumerator UpdateAttackStates()
    {
        yield return new WaitForSeconds(attackBehaviorUpdateInterval);

        float distanceToPlayer = Vector3.Distance(_Character.transform.position, Player.instance.transform.position);

        float totalFrequencies = 0;

        attackCandidates.Clear();

        for(int i = 0; i < attacks.Count; i++)
        {
            EnemyAttackBehaviorData attackData = attacks[i].behaviorData;

            if(distanceToPlayer > attackData.minDistance && distanceToPlayer < attackData.maxDistance)
            {
                // Within range of the attack.
                if(attacks[i].cooldown <= 0 && _AllowedActions[attacks[i].stateScript])
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

            if (attackData.decrementCooldownOnlyWhenAllowed && !_AllowedActions[attacks[i].stateScript]) continue;

            // Handle decrementing cooldowns.
            EnemyAttack attackTemp = attacks[i];
            attackTemp.cooldown -= attackBehaviorUpdateInterval;
            attacks[i] = attackTemp;

            if (attacks[i].cooldown <= 0) _AllowedActions[attacks[i].stateScript] = true;
            else _AllowedActions[attacks[i].stateScript] = true;
        }

        float randomNum = Random.Range(0, totalFrequencies);

        for(int i = 0; i < attackCandidates.Count; i++)
        {
            if(randomNum <= attackCandidates[i].behaviorData.frequency)
            {
                if (StateMachine.TrySetState(attackCandidates[i].stateScript)) ResetCooldown(attackCandidates[i]);
                //Debug.Log("Attempting attack: " + attackCandidates[i].stateScript);
                break;
            }

            randomNum -= attackCandidates[i].behaviorData.frequency;
        }

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
        //Debug.Log("Enemy spotted player!");
        StateMachine.ForceSetState(_InitAggro);
        StateMachine.DefaultState = _InitAggro;
        StartCoroutine(UpdateAttackStates());
    }

#if UNITY_EDITOR
    new void OnValidate()
    {
        base.OnValidate();
        gameObject.GetComponentInParentOrChildren(ref _Pathfinding);
    }
#endif
}
