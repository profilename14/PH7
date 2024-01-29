using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static UnityEngine.GraphicsBuffer;

// Thomas Watson
// Enemy Behavior code


public class EnemyBehavior : MonoBehaviour
{
    [Header("STATS")]
    [Min(1f)]
    public float StartHealth;
    private float CurrentHealth;
    [Range(0f, 14f)]
    public float StartPH;
    private float CurrentPH;


    [Header("DETECTION")]
    public bool PlayerDetected = false;
    public float DetectionRadius;
    private float DetectionDelay = .25f;

    [Header("MOVEMENT")]
    // Reminder: Add an option for what movement mode the enemy should use
    // i.e. Impulse, regular walking, etc.
    public Transform target;

    public MoveMode MovementMode = MoveMode.Walk;
    public float TurnRate = 360f;
    
    // Walk Move Type
    public float WalkSpeed = 0.005f;

    // Impulse Move Type
    public float thrust = 30f;
    public float ThrustDelay = .75f;
    
    // Pathfinding
    private float NextWaypointDistance = 3;
    private bool FacingPlayer;
    private bool ReachedPathEnd;
    private Path path;
    private int CurrentWaypoint = 0;
    private float TimeSinceTargetSeen = 0f;
    private State CurrentState = State.Idle;
    private Vector3 LastKnownPos;
    private Seeker seeker;
    private bool ImpulseActive;

    [Header("ATTACKS")]
    private Coroutine PlayerDetector;
    private Coroutine PursueImpulse;


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

    private void Awake()
    {
        CurrentHealth = StartHealth;
        CurrentPH = StartPH;
        ImpulseActive = false;
        PlayerDetector = StartCoroutine(DetectPlayer());
    }

    // We may want a "Favorite Room" or "Default Position" so that enemies know where to return to if they lose track of a player.
    // That, or they just return to a default idle where they choose a random nearby location and patrol around it.

    void Update()
    {
        Rotation();
        Movement();

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

                //ThrustDelay = .5f;
                //thrust = 10f;

                break;

            case State.Seek:
                //Debug.Log("Seeking...");
                // Set the last known location of the player, then go there. If the player is not seen, return to idle.

                break;

            case State.Attack:
                //Debug.Log("Attacking!");

                //ThrustDelay = 2f;
                //thrust = 30f;

                break;
        }
    }

    private void Rotation()
    {
        var toTarget = path.vectorPath[CurrentWaypoint] - transform.position;
        toTarget.y = 0f;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget), Time.deltaTime * TurnRate);
    }
    private void Movement()
    {
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

        switch (MovementMode)
        {
            case MoveMode.Walk:
                if (ImpulseActive)
                {
                    StopCoroutine(PursueImpulse);
                }

                GetComponent<Rigidbody>().AddForce((path.vectorPath[CurrentWaypoint] - transform.position).normalized * WalkSpeed);
                
                //GetComponent<Rigidbody>.AddForce();
                break;

            case MoveMode.Impulse:
                if (!ImpulseActive)
                {
                    PursueImpulse = StartCoroutine(ImpulsePursuit());
                }
                break;
        }
        
    }

    public void TakeDamage(float damage, float ph, float knockback, Vector3 sourcePos)
    {
        CurrentHealth -= damage;
        CurrentPH += ph;

        Vector3 dir = -((sourcePos - transform.position).normalized);
        Vector3 velocity = dir * knockback;
        GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);

        if (CurrentHealth <= 0) Destroy(this.gameObject);
    }


    IEnumerator DetectPlayer()
    {
        // Detects the player's last known location
        while (true)
        {
            yield return new WaitForSeconds(DetectionDelay);

            Ray ray = new Ray(transform.position, target.transform.position - transform.position);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            
            if (hit.collider.tag == "Player") 
            {
                seeker = GetComponent<Seeker>();
                seeker.StartPath(transform.position, target.position, OnPathComplete);
                LastKnownPos = target.position;
                PlayerDetected = true;
            }
            else
            {
                seeker = GetComponent<Seeker>();
                seeker.StartPath(transform.position, LastKnownPos, OnPathComplete);
                PlayerDetected = false;
            }
        }
    }

    IEnumerator ImpulsePursuit()
    {
        ImpulseActive = true;

        



        while (true)
        {
            yield return new WaitForSeconds(ThrustDelay);
            float angle = 10;
            if (Vector3.Angle(transform.forward, (path.vectorPath[CurrentWaypoint] - transform.position)) < angle)
            {
                if (!ReachedPathEnd)
                {
                    Vector3 dir = (path.vectorPath[CurrentWaypoint] - transform.position).normalized;
                    Vector3 velocity = dir * thrust;

                    GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);
                }
                else
                {
                    // Code to inch closer to the final waypoint
                }
            }

            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 20f, Color.yellow, 1f);
            Debug.Log("Moving to player");



        }
    }
}

// States
public enum State { Inactive, Idle, Seek, Follow, Attack }
public enum MoveMode { Impulse, Walk }