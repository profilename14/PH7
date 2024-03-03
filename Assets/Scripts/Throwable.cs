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
    public int health = 1;

    // Leave this null to not make anything, in the case of say the rock
    [SerializeField] private GameObject destroyEffect; // Anything to make on destruction, (like explosions)



    void Start() {
      //curLifespan = maxLifespan;
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

    private void OnCollisionEnter(Collision other)
    {

        if (other.gameObject.tag == "Enemy") {
          if (!isBeingThrown) {
            return;
          }
          Debug.Log("hi");
          // Ensure this doesn't cause I frames later
          other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(
           -changeInHP, changeInPH, knockback, this.transform.position);


          health -= 1;

          if (health <= 0) {
            Destroy(gameObject);
            if (destroyEffect != null) {
              Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }
          } else {
            isBeingThrown = false;
            isBeingCarried = false;
          }


         }

    }
}
