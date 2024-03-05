using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TelekinesisSpell : MonoBehaviour
{
    private bool isCarryingObject; // False if just summoned and nothings grabbed


    private float curLifespan;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a second
    [HideInInspector] public PlayerCombatController combatController;
    private Throwable heldItem;


    void Start() {
      curLifespan = 0.5f;

    }

    void Update()
    {

      if (!isCarryingObject) {

        if (Input.GetMouseButtonUp(1)) {
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

        heldItem.transform.position = this.transform.position;
        heldItem.transform.rotation = this.transform.rotation;
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

    void TargetLocked(Throwable target) // Throwable target
    {
      //???
      heldItem = target.GetComponent<Throwable>();
      heldItem.Grab();
      Debug.Log("Gottem");
      isCarryingObject = true;
    }

}
