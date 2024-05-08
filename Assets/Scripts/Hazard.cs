using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used by spikes, acid puddles, and alkaline puddles.
public class Hazard : MonoBehaviour
{
    [SerializeField] private float changeInPH;
    [SerializeField] private float damage;
    [SerializeField] private float maxLifespan = 5;
    private float curLifespan;
    [SerializeField] private bool permanent;
    [SerializeField] private bool shrinks;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a second

    private float damageTimer = 0;
    private float damageRate = 0.4f; // How many seconds until the next hit to the enemy happens

    public EnemyAI.DamageSource damageSourceType;

    void Start() {
      curLifespan = maxLifespan;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            if (other.gameObject.GetComponent<EnemyAI>() != null) other.gameObject.GetComponent<EnemyAI>().EnteredPuddle(damage, changeInPH);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if(other.gameObject.GetComponent<EnemyAI>() != null) other.gameObject.GetComponent<EnemyAI>().inPuddle = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("Enemy in puddle!");

        if (other.gameObject.tag == "Player")
        {
            // Drains health and pH for now, should probably respect IFrames for HP later.


            if (damageTimer >= damageRate) {
              if (other.gameObject.GetComponent<MovementController>().isDashing) {
                return; // hazard immunity when dashing (you jump over it)
              }
              //other.gameObject.GetComponent<PlayerStats>().ph += changeInPH * damageRate;
              other.gameObject.GetComponent<PlayerStats>().health -= damage * damageRate;
            }
            if (!permanent) {
              curLifespan -= deltaPhysics;
              if (curLifespan < 0) {
                Destroy(gameObject);
              }
            }



        }
        else if (other.gameObject.tag == "Enemy") {
            // Ensure this doesn't cause I frames later
            /*
          if (damageTimer >= damageRate) {
                Debug.Log("Puddle damage tick! Damage: " + damage * damageRate + " PH: " + changeInPH * damageRate);

                other.gameObject.GetComponent<EnemyAI>().TakeDamage(
             damage * damageRate, changeInPH * damageRate, 0f, new Vector3(0,0,0), damageSourceType);
          }
          if (!permanent) {
            curLifespan -= deltaPhysics;
            if (curLifespan < 0) {
              Destroy(gameObject);
            }
          }*/


      }
      else if (other.gameObject.tag == "HasPH") {
        if (damageTimer >= damageRate) {
          other.gameObject.GetComponent<ObjectWithPH>().ChangePH(changeInPH * damageRate);
        }
        if (!permanent) {
          curLifespan -= deltaPhysics;
          if (curLifespan < 0) {
            Destroy(gameObject);
          }
        }

      }

      if (damageTimer >= damageRate) {
        damageTimer = 0;
      }
      damageTimer += deltaPhysics;

    }

}
