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

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < attacks.Count; i++)
        {
            if (!actionManager.allowedStates.ContainsKey(attacks[i].stateScript)) actionManager.allowedStates.Add(attacks[i].stateScript, true);
        }
    }

    public void SetAllowFollowup(bool isAllowed)
    {
        allowFollowup = isAllowed;
    }

    // Update is called once per frame
    void Update()
    {
        if (allowFollowup)
        {
            foreach (EnemyActionBehavior e in attacks)
            {
                if (IsAttackInRange(e.behaviorData))
                {
                    Debug.Log("Followup");
                    actionManager.allowedStates[e.stateScript] = true;
                    movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
                    movementController.pathfinding.rotationSpeed = character.characterData.rotationSpeed;
                    actionManager.StateMachine.ForceSetState(e.stateScript);
                    allowFollowup = false;
                }
            }
        }
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
}
