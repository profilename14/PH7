using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionLockY : MonoBehaviour
{
    InteractableElevator elevator;
    // Start is called before the first frame update
    bool initialized = false;
    void Start()
    {
        //elevator = transform.parent.gameObject.GetComponent<InteractableElevator>();
    }

    // Update is called once per frame
    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.tag == "Player" && !initialized) {
          Vector3 newPos = new Vector3(transform.position.x,
                               collider.gameObject.transform.position.y,
                               transform.position.z);
          transform.position = newPos;
          initialized = true;
        }
        if (collider.gameObject.tag == "Player" || collider.gameObject.tag == "Enemy") {

          Vector3 newPos = new Vector3(collider.gameObject.transform.position.x,
                               transform.position.y,
                               collider.gameObject.transform.position.z);
          collider.gameObject.transform.position = newPos;
        }

    }

}
