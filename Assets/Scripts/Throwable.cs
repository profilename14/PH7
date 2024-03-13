using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used by spikes, acid puddles, and alkaline puddles.
public class Throwable : MonoBehaviour
{
    [SerializeField] private float changeInPH;
    [SerializeField] private float changeInHP;
    [SerializeField] private float thrownVelocity;
    [SerializeField] private float knockback;
    public bool isBeingThrown;
    public bool isBeingCarried;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a
    public float health = 1;
    public bool breaksOnImpact = true;
    public bool usesOwnPH = false;
    private ObjectWithPH ownPH;

    // Leave this null to not make anything, in the case of say the rock
    [SerializeField] private GameObject destroyEffect; // Anything to make on destruction, (like explosions)



    void Start() {
      //curLifespan = maxLifespan;
      if (usesOwnPH) {
        ownPH = GetComponent<ObjectWithPH>();
      }

    }

    public void Grab() {
      isBeingCarried = true;
      isBeingThrown = false;
    }

    public void Throw() {
      isBeingCarried = false;
      GetComponent<Rigidbody>().AddForce(-transform.forward * thrownVelocity, ForceMode.Impulse);
      isBeingThrown = true;
    }
    public void Drop() {
      isBeingCarried = false;
      isBeingThrown = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "HasPH") {
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

            Debug.Log(other.gameObject.GetComponent<ObjectWithPH>().CurrentPH);

          }

        }
        else if (other.gameObject.tag == "Enemy") {
          if (!isBeingThrown) {
            return;
          }
          // Ensure this doesn't cause I frames later
          if (!usesOwnPH) {
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(
             -changeInHP, changeInPH, knockback, this.transform.position);
          } else {
            float enemyPH = other.gameObject.GetComponent<EnemyBehavior>().getCurPH();

            ownPH.NeutralizePH(enemyPH); // affects damage, so slight differences mean little

            float phChange = Mathf.Abs(enemyPH - ownPH.CurrentPH);

            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(
             -changeInHP, Mathf.Pow(phChange, 1.5f) * changeInPH, knockback, this.transform.position);


          }



          health -= 1;

          if (health <= 0) {
            Destroy(gameObject);
            if (destroyEffect != null) {
              Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }
          } else {
            //isBeingThrown = false;
            isBeingCarried = false;
          }


        }
        else if (other.gameObject.CompareTag("Switch")) {
          Debug.Log("AAAAAAAAAAAAAA");
            if (other.gameObject.GetComponent<Switch>() != null) {
              if (destroyEffect == null) {
                other.gameObject.GetComponent<Switch>().Toggle(); // only rocks activate switches
              }

            }
            if (destroyEffect != null) {
              Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }
            health -= 1f; // Leeway for collision jank
            if (health <= 0) {
              Destroy(gameObject);
            }
        }
        else if ( other.gameObject.CompareTag("AllowsBubble") ) {
          Physics.IgnoreCollision(
            other.gameObject.GetComponent<Collider>(),
            GetComponent<Collider>(), true);
        }
        else if (breaksOnImpact) {
          if (isBeingThrown) {
            isBeingThrown = false;
            health -= 1f; // Leeway for collision jank
            if (health <= 0) {
              Destroy(gameObject);

              if (destroyEffect != null) {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
              }
            }
          }
        }
        else {
          if (isBeingThrown) {
            isBeingThrown = false; // Stops dealing damage after hitting the ground
          }
        }

    }
}
