using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

// Used by spikes, acid puddles, and alkaline puddles.
public class Throwable : MonoBehaviour
{
    [SerializeField] private float changeInPH;
    [SerializeField] private float damage;
    [SerializeField] private float thrownVelocity;
    [SerializeField] private float knockback;
    public bool isBeingThrown;
    public bool isBeingCarried;
    private Vector3 thrownDirection;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a
    public float health = 1;
    public bool breaksOnImpact = true;
    public bool usesOwnPH = false;
    private ObjectWithPH ownPH;

    // Leave this null to not make anything, in the case of say the rock
    [SerializeField] private GameObject destroyEffect; // Anything to make on destruction, (like explosions)
    [SerializeField] private GameObject puddleEffect; // Anything to make on destruction, (like puddles)
    private AudioSource audioSource;
    [SerializeField] private AudioClip enemyImpactSound;

    public EnemyAI.DamageSource damageSourceType;

    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
      //curLifespan = maxLifespan;
      if (usesOwnPH) {
        ownPH = GetComponent<ObjectWithPH>();
      }
      audioSource = GameObject.FindGameObjectWithTag("Sound").GetComponent<AudioSource>();
    }

    public void Grab() {
      isBeingCarried = true;
      isBeingThrown = false;
    }

    public void Throw(Vector3 bubbleAngle) {
      isBeingCarried = false;
      rb.AddForce(bubbleAngle * thrownVelocity, ForceMode.Impulse);
      isBeingThrown = true;
      thrownDirection = bubbleAngle * thrownVelocity;
    }
    public void Drop() {
      isBeingCarried = false;
      isBeingThrown = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "HasPH") {
          Debug.Log(other.gameObject.GetComponent<ObjectWithPH>().CurrentPH);
          if (!isBeingThrown) {
            return;
          }
          // Ensure this doesn't cause I frames later
          if (!usesOwnPH) {
            other.gameObject.GetComponent<ObjectWithPH>().ChangePH(changeInPH);

            
          } else {
            float otherPH = other.gameObject.GetComponent<ObjectWithPH>().CurrentPH;

            ownPH.NeutralizePH(otherPH); // affects damage, so slight differences mean little

            float phChange = Mathf.Abs(otherPH - ownPH.CurrentPH);

            other.gameObject.GetComponent<ObjectWithPH>().ChangePH(Mathf.Pow(phChange, 1.5f) * changeInPH);

            audioSource.PlayOneShot(enemyImpactSound, 0.75F);
          }


        }
        else if (other.gameObject.tag == "Enemy") {
            if (damageSourceType == EnemyAI.DamageSource.Rock || damageSourceType == EnemyAI.DamageSource.Pot)
            {
                if (!isBeingThrown)
                {
                    return;
                }
                
                audioSource.PlayOneShot(enemyImpactSound, 0.95F);
                 

                other.gameObject.GetComponent<EnemyAI>().TakeDamage(damage, changeInPH, knockback, rb.velocity, damageSourceType);

                /*// Ensure this doesn't cause I frames later
                if (!usesOwnPH)
                {
                    other.gameObject.GetComponent<EnemyAI>().TakeDamage(
                     -changeInHP, changeInPH, knockback, this.transform.position, damageSourceType);
                }
                else
                {
                    float enemyPH = other.gameObject.GetComponent<EnemyAI>().armor;

                    ownPH.NeutralizePH(enemyPH); // affects damage, so slight differences mean little

                    float phChange = Mathf.Abs(enemyPH - ownPH.CurrentPH);

                    other.gameObject.GetComponent<EnemyAI>().health -= changeInHP;
                    other.gameObject.GetComponent<EnemyAI>().armor -= changeInPH;


                }*/
            }


            if (this.gameObject.CompareTag("Enemy"))
            {
                //buggy
                //GetComponent<EnemyAI>().TakeDamage(damage, -changeInPH, 0, Vector3.zero, EnemyAI.DamageSource.Rock);
            }

        }
        else if (other.gameObject.CompareTag("Switch")) {
            if (other.gameObject.GetComponent<Switch>() != null) {
              //if (destroyEffect == null) {
                other.gameObject.GetComponent<Switch>().Toggle(); // only rocks activate switches
              //}

            }
        }
        else if ( other.gameObject.CompareTag("AllowsBubble") ) {
          Physics.IgnoreCollision(
            other.gameObject.GetComponent<Collider>(),
            GetComponent<Collider>(), true);
        }

        if(breaksOnImpact && isBeingThrown)
        {
            audioSource.PlayOneShot(enemyImpactSound, 0.75F);
            Destroy(gameObject);
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }
            if (puddleEffect != null)
            {
                Vector3 puddlePos = new Vector3(transform.position.x, -0.85f, transform.position.z);
                puddlePos = puddlePos + (thrownDirection * 0.05f);
                Instantiate(puddleEffect, puddlePos, Quaternion.identity);

            }
        }
    }
}
