using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Thomas Watson
// Enemy Behavior code


public class EnemyBehavior : MonoBehaviour
{
    public GameObject player;
    public GameObject ModelRoot;
    public Transform target;
    public Collider SightSphere;
    public State CurrentState = State.Idle;
    public int MoveSpeed;
    public float DetectionDelay;

    private Coroutine PlayerDetector;
    

    // We may want a "Favorite Room" or "Default Position" so that enemies know where to return to if they lose track of a player.
    // That, or they just return to a default idle where they choose a random nearby location and patrol around it.

    void Update()
    {
        switch (CurrentState)
        {

            case State.Inactive:
                // This state exists to ensure that enemies will not be randomly triggered until the player has reached the appropriate are
                break;

            case State.Idle:
                Debug.Log("Waiting...");
                break;

            case State.Follow:
                Debug.Log("Following!");
                
                // Set the last known location of the player, then go there. If the player is not seen, return to idle.
                // Approach player to within a certain distance, but don't go all the way into a player
                
                // Rotate to face player
                Vector3 relativePos = target.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                ModelRoot.transform.rotation = rotation;

                // Move to player
                // To be replaced by A* pathfinding courtesy of Nick (and Aron Granberg)
                Vector3 direction = (target.position - transform.position).normalized;
                transform.Translate((direction * MoveSpeed) * Time.deltaTime);

                if ( Vector3.Distance(transform.position, target.position) <= 3f )
                {
                    CurrentState = State.Attack;
                }

                break;

            case State.Attack:
                Debug.Log("Attacking!");
                if (Vector3.Distance(transform.position, target.position) > 3f)
                {
                    CurrentState = State.Follow;
                }

                break;
        }    
        

    }

    private void OnTriggerEnter(Collider SightSphere)
    {
        if (SightSphere.gameObject.tag == "Player")
        {
            Debug.Log("Player Entered");
            // Line of Sight handling inspired by William Coyne's article: https://unityscripting.com/line-of-sight-detection/ 
            PlayerDetector = StartCoroutine(DetectPlayer());
        }
    }

    private void OnTriggerExit(Collider SightSphere)
    {
        if (SightSphere.gameObject.tag == "Player")
        {
            Debug.Log("Player Left");
            StopCoroutine(PlayerDetector);
            CurrentState = State.Idle;
        }
    }

    IEnumerator DetectPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(DetectionDelay);

            Ray ray = new Ray(transform.position, target.position - transform.position);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            Debug.DrawRay(ray.origin, ray.direction * 15, Color.red, DetectionDelay);

            if (hit.collider.tag == "Player")
            {
                CurrentState = State.Follow;
            }
            else
            {
                CurrentState = State.Idle;   
            }
        }
    }

}

// States
public enum State { Inactive, Idle, Follow, Attack }