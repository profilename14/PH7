using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Used by spikes, acid puddles, and alkaline puddles.
public class Hazard : MonoBehaviour
{
    [SerializeField] private float pH;
    [SerializeField] private float pHVolume;
    [SerializeField] private float changeInHP;
    [SerializeField] private float maxLifespan = 5;
    private float curLifespan;
    [SerializeField] private bool permanent;
    [SerializeField] private bool shrinks;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a second


    void Start() {
      curLifespan = maxLifespan;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // Drains health and pH for now, should probably respect IFrames for HP later.
            if (other.gameObject.GetComponent<MovementController>().isDashing) {
              return; // hazard immunity when dashing (you jump over it)
            }
            other.gameObject.GetComponent<PlayerStats>().phSubstance.MixWith(pH * deltaPhysics, pH * deltaPhysics);
            other.gameObject.GetComponent<PlayerStats>().health += changeInHP * deltaPhysics;
            if (!permanent) {
              curLifespan -= deltaPhysics;
              if (curLifespan < 0) {
                Destroy(gameObject);
              }
            }


        }
        else if (other.gameObject.tag == "Enemy") {
          // Ensure this doesn't cause I frames later
          other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(
           -changeInHP * deltaPhysics, pH * deltaPhysics, 0f, new Vector3(0,0,0));
          if (!permanent) {
            curLifespan -= deltaPhysics;
            if (curLifespan < 0) {
              Destroy(gameObject);
            }
          }

        }
    }

}
