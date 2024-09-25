using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyAttackBehaviorData : ScriptableObject
{
    // The minimum distance the enemy can be from the player for this attack to be performed.
    public float minDistance;
    
    // The maximum distance the enemy can be from the player for this attack to be performed.
    public float maxDistance;
    
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
}
