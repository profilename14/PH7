using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used by fountains that push away the player (and enemies)
public class Fountain : MonoBehaviour
{
    public float maxDistance = 5; // Try to set this to the outer radius of the trigger
    public float curDistance = 0;
    public float pushPower = 5f;
    [SerializeField] private AnimationCurve PowerFromDistance;



    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            curDistance = (this.transform.position - other.transform.position).magnitude;
            if (curDistance < maxDistance) {
              float curForce = PowerFromDistance.Evaluate(curDistance / maxDistance) * pushPower;
              other.GetComponent<MovementController>().applyKnockback(transform.position, curForce);
            }


        }
        else if (other.gameObject.tag == "Enemy") {
          curDistance = (this.transform.position - other.transform.position).magnitude;
          if (curDistance < maxDistance) {
            EnemyBehavior Enemy = other.GetComponent<EnemyBehavior>();
            Debug.Log("Pushingenemy");

            float curForce = PowerFromDistance.Evaluate(curDistance / maxDistance) * pushPower;

            Enemy.TakeDamage(0, 0, curForce, transform.position); // deal slight knockback every frame.
          }

        }
    }

}
