using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyAttackBehaviorData : ScriptableObject
{
    // The min/max distance the enemy can be from the player for this attack to be performed.
    public FloatMinMax distance;

    // The max distance for the player to be considered line of sight even if the angle measurements fail.
    public float angleOverrideDistance = 2f;

    // The min/max angle between the enemy's forward (+z) vector and the vector from the enemy to the player.
    public FloatMinMax forwardAngle;

    // The min/max angle between the enemy's up (+y) vector and the vector from the enemy to the player.
    public FloatMinMax upAngle;

    // The min/max angle between the enemy's right (+x) vector and the vector from the enemy to the player.
    public FloatMinMax rightAngle;
    
    // Used if there are multiple attacks the enemy can choose at current range.
    // All attack frequencies are added together and normalized out of 1, and a behavior is randomly chosen.
    public float frequency;

    // The cooldown for this attack.
    public float cooldown;

    // The cooldown applied to the enemy during which it cannot use any attacks.
    public float enemyCooldown;

    // If true, then this attack's cooldown starts at max in Awake().
    public bool startWithMaxCooldown;

    // If true, then this attack cooldown is only decremented when within range of the player.
    public bool decrementCooldownOnlyInRange;

    // If true, then this attack cooldown is only decremented when the Enemy is in a state that allows transitioning to it.
    public bool decrementCooldownOnlyWhenAllowed;

    // Required "phase" of an enemy for this attack to be executed (phases are defined in EnemyActionManager). If an enemy is in a phase greater than or equal to this value, the attack is allowed.
    // Ex. a value of 2 means this attack is only allowed in phase 2 or 3
    public int minimumPhase = 1;
}
