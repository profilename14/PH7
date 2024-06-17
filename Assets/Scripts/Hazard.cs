using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

// Used by spikes, acid puddles, and alkaline puddles.
public class Hazard : MonoBehaviour
{
    [SerializeField] private float changeInPH;
    [SerializeField] private float damage;
    [SerializeField] private float maxLifespan = 5;
    [SerializeField] private float curLifespan;
    [SerializeField] private bool permanent;
    [SerializeField] private bool shrinks;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a second

    private float damageTimer = 0;
    private float damageRate = 0.4f; // How many seconds until the next hit to the enemy happens

    
    private float playerDamageTimer = 0;
    private float playerDamageRate = 0.6f; // How many seconds until the next hit to the enemy happens

    public EnemyAI.DamageSource damageSourceType;

    private Vector3 startScale;
    private Vector3 newScale;
    [SerializeField] private bool damagesAlkaline = false;
    [SerializeField] private bool damagesPlayer = false;

    void Start() {
      curLifespan = maxLifespan;
      startScale = transform.parent.localScale;
      newScale = startScale;
    }

    void Update() {
      if (!permanent) {
        curLifespan -= Time.deltaTime;
        newScale = startScale * (1 - Mathf.Exp(-4 * (curLifespan / maxLifespan)));
        transform.parent.localScale = newScale;

        if (curLifespan < maxLifespan / 5) {
          curLifespan -= Time.deltaTime;
        }

        if (curLifespan < 0) {
          Destroy(gameObject);
        }
      }
      if (playerDamageTimer > 0) {
        playerDamageTimer -= Time.deltaTime * 0.2f;
      }
      
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            if (other.gameObject.GetComponent<EnemyAI>() != null && damagesAlkaline && other.gameObject.GetComponent<EnemyAI>().naturalPH == TypesPH.Acidic) {
              return; // Don't do damage to acidic enemies if your a spike puddle
            }
            if (other.gameObject.GetComponent<EnemyAI>() != null) other.gameObject.GetComponent<EnemyAI>().EnteredPuddle(damage, changeInPH);
        }
        if (other.gameObject.tag == "Player")
        {
          if (changeInPH < 0) {
            other.gameObject.GetComponent<PlayerStats>().inAcid = true;
            other.gameObject.GetComponent<PlayerStats>().acidLink = this;
          } else {
            other.gameObject.GetComponent<PlayerStats>().inAlkaline = true;
            other.gameObject.GetComponent<PlayerStats>().alkalineLink = this;
          }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if(other.gameObject.GetComponent<EnemyAI>() != null) other.gameObject.GetComponent<EnemyAI>().inPuddle = false;
        }
        if (other.gameObject.tag == "Player")
        {
          if (changeInPH < 0) {
            other.gameObject.GetComponent<PlayerStats>().inAcid = false;
            if (other.gameObject.GetComponent<PlayerStats>().acidLink == this) {
              other.gameObject.GetComponent<PlayerStats>().acidLink = null;
            }
          } else {
            other.gameObject.GetComponent<PlayerStats>().inAlkaline = false;
            if (other.gameObject.GetComponent<PlayerStats>().alkalineLink == this) {
              other.gameObject.GetComponent<PlayerStats>().alkalineLink = null;
            }
            
          }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("Enemy in puddle!");

        if (other.gameObject.tag == "Player")
        {
            // Drains health and pH for now, should probably respect IFrames for HP later.


            if (playerDamageTimer >= playerDamageRate && damagesPlayer) {
              if (other.gameObject.GetComponent<MovementController>().isDashing) {
                return; // hazard immunity when dashing (you jump over it)
              }
              //other.gameObject.GetComponent<PlayerStats>().ph += changeInPH * damageRate;
              if (damagesPlayer == true) {
                if (other.gameObject.GetComponent<PlayerStats>().health > 1.1) {
                  other.gameObject.GetComponent<PlayerStats>().health -= 1;
                  playerDamageTimer = 0;
                }
                
              }
            }
            else {
              playerDamageTimer += Time.deltaTime;
            }
            if (!permanent) {
              //curLifespan -= deltaPhysics;
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
        other.gameObject.GetComponent<ObjectWithPH>().ChangePH(changeInPH * damageRate / 14f);
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

    public void spendPuddle() {
      if (permanent == false) {
        curLifespan -= 0.5f;
      }
    }

}
