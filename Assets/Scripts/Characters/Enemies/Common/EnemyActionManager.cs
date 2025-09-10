using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Animancer;
using Animancer.FSM;

[System.Serializable]
public struct EnemyActionBehavior
{
    public float cooldown;
    //public CharacterActionPriority priority;
    public CharacterState stateScript;
    public EnemyAttackBehaviorData behaviorData;
}

public class EnemyActionManager : CharacterActionManager
{
    // For storing a reference to the Player singleton instance.
    protected Player player;

    [SerializeField]
    protected CharacterState _HitStun;

    [SerializeField]
    // How often (in seconds) should this Enemy update attack behavior?
    protected float attackBehaviorUpdateInterval = 0.1f;

    [SerializeField]
    protected bool canAttackInIdle;

    [SerializeField]
    protected List<EnemyActionBehavior> attacks = new();

    [SerializeField]
    protected float noAttacksTimer;

    private List<EnemyActionBehavior> attackCandidates = new();

    [SerializeField]
    protected bool isStunned = false;

    [SerializeField]
    protected EnemyMovementController movementController;

    [SerializeField]
    private ClipTransition deathAnimation;

    protected override void Awake()
    {
        base.Awake();

        gameObject.GetComponentInParentOrChildren(ref movementController);

        // Set up everything for all the attack behaviors.
        for (int i = 0; i < attacks.Count; i++)
        {
            if (attacks[i].behaviorData.startWithMaxCooldown)
            {
                allowedStates.Add(attacks[i].stateScript, false);
                EnemyActionBehavior attackTemp = attacks[i];
                attackTemp.cooldown = attacks[i].behaviorData.cooldown;
                attacks[i] = attackTemp;
            }
            else
            {
                if(!allowedStates.ContainsKey(attacks[i].stateScript)) allowedStates.Add(attacks[i].stateScript, true);
            }
        }
    }

    protected void Start()
    {
        player = Player.instance;
        if (canAttackInIdle) StartCoroutine(UpdateAttackStates());
    }

    protected void Update()
    {
        
    }

    public IEnumerator UpdateAttackStates()
    {
        Vector3 vectorToPlayer = Player.instance.transform.position - character.transform.position;

        Vector3 vectorToPlayerSameY = new Vector3(Player.instance.transform.position.x - character.transform.position.x, character.transform.forward.y, Player.instance.transform.position.z - character.transform.position.z);

        float distanceToPlayer = vectorToPlayer.magnitude;
        float angleToPlayerForward = Vector3.Angle(character.transform.forward, vectorToPlayerSameY);
        float angleToPlayerUp = Vector3.Angle(character.transform.up, vectorToPlayer);
        float angleToPlayerRight = Vector3.Angle(character.transform.right, vectorToPlayer);

        float totalFrequencies = 0;

        attackCandidates.Clear();

        for (int i = 0; i < attacks.Count; i++)
        {
            EnemyAttackBehaviorData attackData = attacks[i].behaviorData;

            if (attackData.decrementCooldownOnlyWhenAllowed && !allowedStates[attacks[i].stateScript]) continue;

            //Debug.Log(distanceToPlayer + " : so distance is within range -- " + (distanceToPlayer >= attackData.distance.min && distanceToPlayer < attackData.distance.max));

            //Debug.Log(angleToPlayerForward + " : so forward angle is within range -- " + (angleToPlayerForward >= attackData.forwardAngle.min && angleToPlayerForward < attackData.forwardAngle.max));

            //Debug.Log(angleToPlayerUp + " : so up angle is within range -- " + (angleToPlayerUp >= attackData.upAngle.min && angleToPlayerUp < attackData.upAngle.max));

            //Debug.Log(angleToPlayerRight + " : so right angle is within range -- " + (angleToPlayerRight >= attackData.rightAngle.min && angleToPlayerRight < attackData.rightAngle.max));

            if (distanceToPlayer >= attackData.distance.min && distanceToPlayer < attackData.distance.max
                && angleToPlayerForward >= attackData.forwardAngle.min && angleToPlayerForward < attackData.forwardAngle.max
                && angleToPlayerUp >= attackData.upAngle.min && angleToPlayerUp < attackData.upAngle.max
                && angleToPlayerRight >= attackData.rightAngle.min && angleToPlayerRight < attackData.rightAngle.max)
            {
                //Debug.Log("Within attack range");

                // Within range of the attack.
                if (attacks[i].cooldown <= 0 && allowedStates[attacks[i].stateScript])
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
            EnemyActionBehavior attackTemp = attacks[i];
            attackTemp.cooldown -= attackBehaviorUpdateInterval;
            attacks[i] = attackTemp;

            if (!isStunned && attacks[i].cooldown <= 0) allowedStates[attacks[i].stateScript] = true;
            else allowedStates[attacks[i].stateScript] = false;
        }

        if (!isStunned)
        {
            float randomNum = Random.Range(0, totalFrequencies);

            for (int i = 0; i < attackCandidates.Count; i++)
            {
                if (randomNum <= attackCandidates[i].behaviorData.frequency)
                {
                    if (StateMachine.TrySetState(attackCandidates[i].stateScript)) ResetCooldown(attackCandidates[i]);
                    //Debug.Log("Attempting attack: " + attackCandidates[i].stateScript);
                    break;
                }

                randomNum -= attackCandidates[i].behaviorData.frequency;
            }
        }

        yield return new WaitForSeconds(attackBehaviorUpdateInterval);

        StartCoroutine(UpdateAttackStates());
    }

    public void ResetCooldown(EnemyActionBehavior behavior)
    {
        for(int i = 0; i < attacks.Count; i++)
        {
            if(attacks[i].Equals(behavior))
            {
                EnemyActionBehavior attackTemp = attacks[i];
                attackTemp.cooldown = attacks[i].behaviorData.cooldown;
                attacks[i] = attackTemp;
            }
        }
    }

    public override void Hitstun()
    {
        if (StateMachine.TryResetState(_HitStun))
        {
            //Debug.Log("Hitstun!");
            isStunned = true;
            movementController.SetAIEnabled(false);
        }
    }

    public override void EndHitStun()
    {
        isStunned = false;
        //Debug.Log("End stun");
        movementController.SetAIEnabled(true);
    }

    public void OnDeath()
    {
        /*if (deathAnimation != null)
        {
            StateMachine.SetAllowNullStates(true);
            StateMachine.ForceSetState(null);
            anim.Play(deathAnimation);
        }*/
    }

#if UNITY_EDITOR
    new void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
