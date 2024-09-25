using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used by the neutral rain Typhis can summon. Acts similar to hazards.
public class HydroxideSpell : MonoBehaviour
{
    [SerializeField] private float drainRatePH = 8;
    [SerializeField] private float maxLifespan = 0.10f;
    private float curLifespan;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a second
    public PlayerStatsOLD playerStats;


    void Start() {
      curLifespan = maxLifespan;

    }

    void Update()
    {
      curLifespan -= Time.deltaTime;
      if (curLifespan < 0) {
        Destroy(gameObject);
      }

      if (playerStats != null) { // When were sure we've linked the player to the spell:
        Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale / 2, Quaternion.identity);
        EnemyBehavior target = null;
        float targetValue = 0;

        foreach (var other in hitColliders)
        {

          if (other.gameObject.tag == "Enemy") {

            if (target == null) {
              target = other.gameObject.GetComponent<EnemyBehavior>();
              targetValue = target.getCurPH();
              /*if (target.stunRecoveryTimer > 0 && target.stunTimer <= 0) {
                targetValue = -1;
              }*/
            }
            else {
              EnemyBehavior option = other.gameObject.GetComponent<EnemyBehavior>();
              float optionValue = option.getCurPH();
              /*if (option.stunRecoveryTimer > 0 && option.stunTimer <= 0) {
                optionValue = -1;
              }*/

              if (optionValue > targetValue) {
                target = option;
              }
            }

          }
        }

        if (target != null) {
          TargetLocked(target);

          Destroy(gameObject); // Single use
        }

      }

    }

    void TargetLocked(EnemyBehavior target)
    {
      // Ensure this doesn't cause I frames later
      float opponentPH = target.getCurPH();
      //float pHDifference = opponentPH - playerStats.ph;
      float multiplier = 1;

      if (opponentPH > 7) {
        opponentPH = 14; // To make healing feel less random
      } else if (opponentPH < 7 && opponentPH > 2) {
        opponentPH = 5; // about 1/3 effectiveness on an alkalized foe
      } else if (opponentPH <= 2) {
        opponentPH = 2; // about 1/7 effectiveness if you didn't even bubble them
      } else if (opponentPH == 7) {
        opponentPH = 7; // if the opponent is currently stunned, 1/2 effectiveness
      }



      /*if (pHDifference >= 0) {
        multiplier = 1 + 0.02f * Mathf.Pow(pHDifference, 1.496f); // 1x - 2x
      } else {
        multiplier = 1 * Mathf.Pow( (-pHDifference + 1), -0.5f); // 1-15 ^ -0.5 = 1x to 0.25x
      }*/

      /*if (target.stunTimer <= 0
       && target.stunRecoveryTimer > 0 ) {

        multiplier = 0; // When other is recovering from stun, you can't hydroxide drain for PH
        // I'll leave it allowed for now when they are stunned, so you can ph drain a neutralized
        // acidic foe (while you only get half as much anyway for neutralize alkaline foes).
      }*/

      target.TakeDamage(0f, -drainRatePH * multiplier, 0f, new Vector3(0,0,0));

      //playerStats.ph += drainRatePH * multiplier;

      //if (playerStats.ph > 14) {
        //playerStats.hydroxidePower = true;
      //}
    }

}
