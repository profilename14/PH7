using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used by the neutral rain Typhis can summon. Acts similar to hazards.
public class HydroxideSpell : MonoBehaviour
{
    [SerializeField] private float drainRatePH = 8;
    [SerializeField] private float maxLifespan = 0.75f;
    private float curLifespan;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a second
    public PlayerStats playerStats;


    void Start() {
      curLifespan = maxLifespan;
    }

    void Update() {
      curLifespan -= Time.deltaTime;
      if (curLifespan < 0) {
        Destroy(gameObject);
      }
    }

    private void OnTriggerStay(Collider other)
    {
        if (playerStats == null) {
          Destroy(gameObject); // To be safe
        }

        if (other.gameObject.tag == "Enemy") {

          // Ensure this doesn't cause I frames later
          float opponentPH = other.gameObject.GetComponent<EnemyBehavior>().getCurPH();
          float pHDifference = opponentPH - playerStats.ph;
          float multiplier = 1;

          if (opponentPH > 7) {
            opponentPH = 14; // To make healing feal less random
          }

          if (pHDifference >= 0) {
            multiplier = 1 + 0.057f * Mathf.Pow(pHDifference, 1.496f);
          } else {
            multiplier = 1 * Mathf.Pow( (-pHDifference + 1), -0.5f); // 1-15 ^ -0.5 = 1x to 0.25x
          }

          if (other.gameObject.GetComponent<EnemyBehavior>().stunTimer <= 0
           && other.gameObject.GetComponent<EnemyBehavior>().stunRecoveryTimer > 0 ) {

            multiplier = 0; // When other is recovering from stun, you can't hydroxide drain for PH
            // I'll leave it allowed for now when they are stunned, so you can ph drain a neutralized
            // acidic foe (while you only get half as much anyway for neutralize alkaline foes).
          }

          other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(
            0f, -drainRatePH * deltaPhysics * multiplier, 0f, new Vector3(0,0,0));

          playerStats.ph += drainRatePH * deltaPhysics * multiplier;

          if (playerStats.ph > 14) {
            playerStats.hydroxidePower = true;
          }

        }
    }

}
