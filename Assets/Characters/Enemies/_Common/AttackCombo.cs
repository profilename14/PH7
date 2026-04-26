using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCombo : MonoBehaviour
{
    // Component that allows for follow up attacks
    [SerializeField]
    Character character;

    [SerializeField]
    EnemyActionManager actionManager;

    [SerializeField]
    EnemyMovementController movementController;

    [SerializeField] CharacterState currentAttack;

    [SerializeField]
    protected List<EnemyActionBehavior> attacks = new();

    public bool allowFollowup;

    [SerializeField]
    // How often (in seconds) should this Enemy update attack behavior?
    protected float attackBehaviorUpdateInterval = 0.1f;

    private List<EnemyActionBehavior> attackCandidates = new();

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < attacks.Count; i++)
        {
            if (attacks[i].behaviorData.startWithMaxCooldown)
            {
                if (!actionManager.allowedStates.ContainsKey(attacks[i].stateScript)) actionManager.allowedStates.Add(attacks[i].stateScript, false);
                EnemyActionBehavior attackTemp = attacks[i];
                attackTemp.cooldown = attacks[i].behaviorData.cooldown;
                attacks[i] = attackTemp;
            }
            else
            {
                if (!actionManager.allowedStates.ContainsKey(attacks[i].stateScript)) actionManager.allowedStates.Add(attacks[i].stateScript, true);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(UpdateAttackStates());
    }

    public void SetAllowFollowup(bool isAllowed)
    {
        allowFollowup = isAllowed;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator UpdateAttackStates()
    {
        if (!allowFollowup)
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                // Handle decrementing cooldowns.
                EnemyActionBehavior attackTemp = attacks[i];
                attackTemp.cooldown -= attackBehaviorUpdateInterval;
                attacks[i] = attackTemp;
            }

            yield return new WaitForSeconds(attackBehaviorUpdateInterval);

            StartCoroutine(UpdateAttackStates());

            yield break;
        }

        Debug.Log("Checking followups");

        float totalFrequencies = 0;

        attackCandidates.Clear();

        for (int i = 0; i < attacks.Count; i++)
        {
            EnemyAttackBehaviorData attackData = attacks[i].behaviorData;

            if (attackData.decrementCooldownOnlyWhenAllowed && !actionManager.allowedStates[attacks[i].stateScript]) continue;

            if (attackData.minimumPhase > actionManager.currentPhase) continue;

            if (IsAttackInRange(attackData))
            {
                Debug.Log("Within attack range");

                // Within range of the attack.
                if (attacks[i].cooldown <= 0)
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

            //if (attacks[i].cooldown <= 0) actionManager.allowedStates[attacks[i].stateScript] = true;
            //else actionManager.allowedStates[attacks[i].stateScript] = false;
        }

        if (!actionManager.isStunned)
        {
            float randomNum = Random.Range(0, totalFrequencies);

            IListExtensions.Shuffle(attackCandidates);

            for (int i = 0; i < attackCandidates.Count; i++)
            {
                if (randomNum <= attackCandidates[i].behaviorData.frequency)
                {
                    actionManager.allowedStates[attackCandidates[i].stateScript] = true;
                    movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
                    movementController.pathfinding.rotationSpeed = character.characterData.rotationSpeed;

                    actionManager.StateMachine.ForceSetState(attackCandidates[i].stateScript);

                    Debug.Log("Attempting followup: " + attackCandidates[i].stateScript);
                    //Debug.Log(allowedStates[attackCandidates[i].stateScript]);
                    //Debug.Log(allowedActionPriorities[CharacterActionPriority.Medium]);
                    ResetCooldown(attackCandidates[i]);
                    allowFollowup = false;

                    break;
                }

                randomNum -= attackCandidates[i].behaviorData.frequency;
            }
        }

        yield return new WaitForSeconds(attackBehaviorUpdateInterval);

        StartCoroutine(UpdateAttackStates());
    }

    public bool IsAttackInRange(EnemyAttackBehaviorData attackData)
    {
        Vector3 vectorToPlayer = Player.instance.transform.position - character.transform.position;

        Vector3 vectorToPlayerSameY = new Vector3(Player.instance.transform.position.x - character.transform.position.x, character.transform.forward.y, Player.instance.transform.position.z - character.transform.position.z);

        float distanceToPlayer = vectorToPlayer.magnitude;
        float angleToPlayerForward = Vector3.Angle(character.transform.forward, vectorToPlayerSameY);
        float angleToPlayerUp = Vector3.Angle(character.transform.up, vectorToPlayer);
        float angleToPlayerRight = Vector3.Angle(character.transform.right, vectorToPlayer);

        if (distanceToPlayer >= attackData.distance.min && distanceToPlayer < attackData.distance.max
                && angleToPlayerForward >= attackData.forwardAngle.min && angleToPlayerForward < attackData.forwardAngle.max
                && angleToPlayerUp >= attackData.upAngle.min && angleToPlayerUp < attackData.upAngle.max
                && angleToPlayerRight >= attackData.rightAngle.min && angleToPlayerRight < attackData.rightAngle.max)
        {
            return true;
        }
        else return false;
    }

    public void ResetCooldown(EnemyActionBehavior behavior)
    {
        for (int i = 0; i < attacks.Count; i++)
        {
            if (attacks[i].Equals(behavior))
            {
                EnemyActionBehavior attackTemp = attacks[i];
                attackTemp.cooldown = attacks[i].behaviorData.cooldown;
                attacks[i] = attackTemp;
            }
        }
    }
}
