using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Pathfinding;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

// Thomas Watson
// Enemy Behavior code


public class EnemyBehavior : MonoBehaviour
{
    [Header("STATS")]
    public float health;
    public float ph;

    [Header("DETECTION")]
    public Collider SightSphere;
    public float DetectionDelay;
    public float ThrustDelay;

    [Header("MOVEMENT")]
    // Reminder: Add an option for what movement mode the enemy should use
    // i.e. Impulse, regular walking, etc.
    public Transform TargetPosition;
    public float thrust = 20f;
    public float NextWaypointDistance = 3;
    public bool ReachedPathEnd;
    public Path path;
    private int CurrentWaypoint = 0;
    
    private State CurrentState = State.Idle;

    private Coroutine PlayerDetector;
    private Coroutine PlayerPersuit;

    private Seeker seeker;

    public void OnPathComplete(Path p)
    {

        if (!p.error)
        {
            path = p;
            CurrentWaypoint = 0;
        }
        else
        {
            Debug.Log("Error in Pathing:" + p.error);
        }
    }

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
                //Debug.Log("Waiting...");
                break;

            case State.Follow:
                //Debug.Log("Following!");

                ThrustDelay = .5f;
                thrust = 10f;


                break;

            case State.Seek:
                //Debug.Log("Seeking...");
                // Set the last known location of the player, then go there. If the player is not seen, return to idle.

                break;

            case State.Attack:
                //Debug.Log("Attacking!");

                ThrustDelay = 2f;
                thrust = 30f;

                break;
        }    
        

    }

    private void OnTriggerEnter(Collider SightSphere)
    {
        // This code activates when the player enters the "Enemy Sight Sphere" object;
        // Is this the way we want to approach this? It could lead to some potential problems

        if (SightSphere.gameObject.tag == "Player")
        {
            Debug.Log("Player Entered");

            // Line of Sight handling inspired by William Coyne's article: https://unityscripting.com/line-of-sight-detection/ 
            PlayerDetector = StartCoroutine(DetectPlayer());

            PlayerPersuit = StartCoroutine(PersuePlayer());
        }
    }

    private void OnTriggerExit(Collider SightSphere)
    {
        if (SightSphere.gameObject.tag == "Player")
        {
            Debug.Log("Player Left");
            StopCoroutine(PlayerDetector);

            StopCoroutine(PlayerPersuit);
        }
    }

    IEnumerator DetectPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(DetectionDelay);

            Ray ray = new Ray(transform.position, TargetPosition.transform.position - transform.position);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            
            if (hit.collider.tag == "Player")
            {
                seeker = GetComponent<Seeker>();
                seeker.StartPath(transform.position, TargetPosition.position, OnPathComplete);
            }
        }
    }

    IEnumerator PersuePlayer()
    {
        while (true)
        {
            Debug.Log("Moving to player");
            yield return new WaitForSeconds(ThrustDelay);

            ReachedPathEnd = false;

            float DistanceToWaypoint;

            while (true)
            {
                DistanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[CurrentWaypoint]);
                if (DistanceToWaypoint < NextWaypointDistance)
                {
                    if (CurrentWaypoint + 1 < path.vectorPath.Count)
                    {
                        CurrentWaypoint++;
                    }
                    else
                    {
                        ReachedPathEnd = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            Vector3 dir = (path.vectorPath[CurrentWaypoint] - transform.position).normalized;
            Vector3 velocity = dir * thrust;

            GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);
        }
    }

}

// States
public enum State { Inactive, Idle, Seek, Follow, Attack }