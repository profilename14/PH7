using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TelekinesisSpell : MonoBehaviour
{
    private bool isCarryingObject; // False if just summoned and nothing's grabbed

    private float curLifespan;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a second
    [HideInInspector] public PlayerCombatController combatController;
    private Throwable heldItem;
    private Vector3 throwableOffset;
    private Rigidbody itemRigidbody;
    public Collider TyphisCollider;

    void Start() {
      curLifespan = 0.5f;
      throwableOffset = new Vector3(0, 0.1f, 0);
    }

    void Update()
    {

      if (!isCarryingObject) {

        if (Input.GetMouseButtonDown(1)) {
          combatController.objectWasThrown();
          Destroy(gameObject);
        }

        curLifespan -= Time.deltaTime;
        if (curLifespan < 0) {
          //Destroy(gameObject);
        }

        if (combatController != null) { // When were sure we've linked the player to the spell:
          Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale / 2, Quaternion.identity);
          Throwable target = null;
          float targetValue = 0;

          foreach (var other in hitColliders)
          {

            if (target == null) {
              // !Still null if theres no throwable!
              target = other.gameObject.GetComponent<Throwable>();

              if (target != null) {
                break;
              }
            }

          }

          if (target != null) {
            TargetLocked(target);
          }
          else {
            //Destroy(gameObject); // Bubble pops as there's nothing to grab
          }
      }
    } else { // end of code for not having an object
        // Lifespan stops ticking if this isn't the case

        if (heldItem == null) {
          combatController.objectWasThrown();
          Destroy(gameObject);
        }
        heldItem.transform.rotation = this.transform.rotation;
        heldItem.transform.position = this.transform.position;

        Vector3 destination = this.transform.position;

        // Velocity is the vector required to reach the middle of the bubble, allowing smooth movement that respects collision
        //For right now I disabled this since it looks a bit jittery and pushes the current pot prefabs out of the bubble. We should revisit this when we revise the bubble code. -Nick
            //itemRigidbody.velocity = 20f * (destination - heldItem.transform.position);


        if (Input.GetMouseButtonDown(0)) {

          Debug.Log("Thrown");
          heldItem.Throw();
          combatController.objectWasThrown();
          Destroy(gameObject);
        }
        if (Input.GetMouseButtonDown(1)) {

          Debug.Log("Dropped");
          heldItem.Drop();
          combatController.objectWasThrown();
          Destroy(gameObject);
        }

      }








    }

    void TargetLocked(Throwable target)
    {
      //This code runs when there's a valid target after a delay, and does initialization so that the rest of the code can run
      //(It has to wait for the CombatController to send over parameters before running)
      heldItem = target.GetComponent<Throwable>();
      itemRigidbody = target.GetComponent<Rigidbody>();
      heldItem.Grab();
      Physics.IgnoreCollision(TyphisCollider, target.GetComponent<Collider>());
      target.transform.position = new Vector3(target.transform.position.x, 
                                              this.transform.position.y + throwableOffset.y, target.transform.position.z);
      Debug.Log("Gottem");
      isCarryingObject = true;
    }

}
