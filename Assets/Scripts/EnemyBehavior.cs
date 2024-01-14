using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Pathfinding;
using UnityEditor.Experimental.GraphView;

// Thomas Watson
// Enemy Behavior code


public class EnemyBehavior : MonoBehaviour
{
    public Collider SightSphere;
    public State CurrentState = State.Idle;
    public float DetectionDelay;
    public float ThrustDelay;
    
    public float thrust = 160f;
    public float NextWaypointDistance = 3;
    private int CurrentWaypoint = 0;
    public bool ReachedPathEnd;

    private Coroutine PlayerDetector;
    private Coroutine PlayerPersuit;

    private Seeker seeker;

    public Transform TargetPosition;
    public Path path;


    public void Start()
    {
        
    }

    public void OnPathComplete(Path p)
    {
        Debug.Log("Path complete" + p.error);

        if (!p.error)
        {
            path = p;
            CurrentWaypoint = 0;
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

                //player.GetComponent<AIDestinationSetter>().target = LastSeen;
                break;

            case State.Seek:
                //Debug.Log("Seeking...");
                // Set the last known location of the player, then go there. If the player is not seen, return to idle.

                // Approach player to within a certain distance, but don't go all the way into a player




                break;

            case State.Attack:
                //Debug.Log("Attacking!");

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
            //CurrentState = State.Idle;
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

                Debug.DrawRay(ray.origin, ray.direction * 15, Color.red, DetectionDelay);
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

            // Random stuff
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

            GetComponent<Rigidbody>().AddForce(velocity);
        }
    }

}

// States
public enum State { Inactive, Idle, Seek, Follow, Attack }