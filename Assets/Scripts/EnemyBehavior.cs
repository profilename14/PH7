using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
//using UnityEditor.Experimental.GraphView;
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
    public float RegenPH = 0.33f; // This much H regened toward default per second.
    private float RegenPHTimer = 0.0f;
    private float RegenPHCooldown = 2.0f; // How long after a pH attack regen is disabled


    [Header("DETECTION")]
    public bool PlayerDetected = false;
    //public float DetectionRadius;
    private float DetectionDelay = .25f;

    [Header("MOVEMENT")]
    // Reminder: Add an option for what movement mode the enemy should use
    // i.e. Impulse, regular walking, etc.
    public Transform target;

    public MoveMode MovementMode = MoveMode.Walk;
    public float TurnRate = 360f;

    // Used by war striders. Might turn into a special behavior id later.
    public bool doubleDash = false;
    private bool dashCombo = false; // Used to tell if the second dash should happen sooner.

    // Walk Move Type
    public float WalkSpeed = 0.005f;

    // Impulse Move Type
    public float thrust = 30f;
    public float ThrustDelay = .75f;
    public float randomFactorRange = 0.1f;

    //Animation-related
    public float animDelay;
    public Animator anim;

    // Pathfinding
    public float NextWaypointDistance = 3;
    private bool ReachedPathEnd;
    private Path path;
    private int CurrentWaypoint = 0;
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
            //Debug.Log("Error in Pathing:" + p.error);
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
        if (CurrentState != State.Idle) { // If the enemy hasn't seen the player
          Rotation();
          Movement();
        }

        if (RegenPHTimer > 0) {
          RegenPHTimer -= Time.deltaTime;
        } else {
          if (CurrentPH < StartPH) {
            CurrentPH += RegenPH * Time.deltaTime;
            if (CurrentPH > StartPH) {
              CurrentPH = StartPH;
            }
          } else if (CurrentPH > StartPH) {
            CurrentPH -= RegenPH * Time.deltaTime;
            if (CurrentPH < StartPH) {
              CurrentPH = StartPH;
            }
          }
        }


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
        if (path == null) { // spam preventer
          return;
        }

        var toTarget = path.vectorPath[CurrentWaypoint] - transform.position;
        toTarget.y = 0f;

        //If statement (which is likely to be a little slow) required to prevent a different "zero" spam error.
        if (toTarget != new Vector3(0,0,0)) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget), Time.deltaTime * TurnRate);
        }

    }
    private void Movement()
    {
        ReachedPathEnd = false;

        float DistanceToWaypoint;

        while (true)
        {
            if (path == null) { // spam prevention
              break;
            }
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
                NextWaypointDistance = 1;

                if (ImpulseActive)
                {
                    StopCoroutine(PursueImpulse);
                }
                if (path != null) { // preventing spam
                  GetComponent<Rigidbody>().AddForce((path.vectorPath[CurrentWaypoint] - transform.position).normalized * WalkSpeed);
                }

                //GetComponent<Rigidbody>.AddForce();
                break;

            case MoveMode.Impulse:
                NextWaypointDistance = 3;

                if (!ImpulseActive)
                {
                    PursueImpulse = StartCoroutine(ImpulsePursuit());
                }
                break;
        }

    }

    public void TakeDamage(float damage, float ph, float knockback, Vector3 sourcePos)
    {

        CurrentPH += ph;

        if (CurrentPH > 14) {
          CurrentPH = 14;
        } else if (CurrentPH < 0) {
          CurrentPH = 0;
        }

        // pH formula: (1 + 0.057 * x^1.496) times damage
        float pHDifference = Mathf.Abs(StartPH - CurrentPH);
        float multiplier = 1 + 0.057f * Mathf.Pow(pHDifference, 1.496f);
        CurrentHealth -= damage * multiplier;
        float displayedMultiplier = Mathf.Round(multiplier * 10.0f) * 0.1f; // Rounded to 1 decimal

        if (ph != 0) {
          RegenPHTimer = RegenPHCooldown;
        }

        if (damage > 0) {
          Debug.Log("Damage: " + damage + " w/ multiplier " + displayedMultiplier  + " to pH of " + pHDifference + "Dif");

        }


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

            seeker = GetComponent<Seeker>();

            Ray ray = new Ray(transform.position, target.transform.position - transform.position);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 50f, 1 << LayerMask.NameToLayer("BlocksVision"));

            if (hit.collider)
            {
                seeker.StartPath(transform.position, LastKnownPos, OnPathComplete);
                PlayerDetected = false;
            }
            else
            {
                seeker.StartPath(transform.position, target.position, OnPathComplete);
                LastKnownPos = target.position;
                PlayerDetected = true;
            }
        }
    }

    IEnumerator ImpulsePursuit()
    {
        ImpulseActive = true;

        while (true)
        {
            yield return new WaitForSeconds(ThrustDelay + Random.Range(-randomFactorRange, randomFactorRange));
            float angle = 10;
            if (path == null) {
              // Lets just wait for now.to avoid spam
            }
            else if (Vector3.Angle(transform.forward, (path.vectorPath[CurrentWaypoint] - transform.position)) < angle)
            {
                if (!ReachedPathEnd)
                {
                    Vector3 dir = (path.vectorPath[CurrentWaypoint] - transform.position).normalized;
                    Vector3 velocity = dir * thrust;

                    anim.SetTrigger("Charge");
                    yield return new WaitForSeconds(animDelay);
                    GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);

                    if (doubleDash) { // If we're a war strider
                      if (dashCombo == false) { // If we did an initial dash
                        ThrustDelay = ThrustDelay / 1.5f;
                        TurnRate = TurnRate * 2.5f;
                        dashCombo = true;
                      } else { // If we're wrapping up our second dash.
                        ThrustDelay = ThrustDelay * 1.5f;
                        TurnRate = TurnRate / 2.5f;
                        dashCombo = false;
                      }
                    }
                }
                else
                {
                    // Code to inch closer to the final waypoint
                }
            }

            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 20f, Color.yellow, 1f);
        }
    }

    public float getHealth() {
      return CurrentHealth;
    }

    public float getCurPH() {
      return CurrentPH;
    }

    // For dash pH increase/damage
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // For debug: Running into an enemy wakes it up if has no door to guard.
            AlertEnemy();

            // detect if the player is dashing. The aftermath does less damage.
            // Primarily increases pH and has high knockback.
            if (other.gameObject.GetComponent<MovementController>().isDashing) {
              if (!other.gameObject.GetComponent<MovementController>().dashEnding)
              {
                TakeDamage(3, 1.5f, 7, other.gameObject.transform.position);
              }
              else
              {
                TakeDamage(2, 1f, 3.5f, other.gameObject.transform.position);
              }
            }

        }
    }

    public void AlertEnemy() { // To be called by any trigger for a door the enemy is guarding.
        CurrentState = State.Follow;
    }
}

// States
public enum State { Inactive, Idle, Seek, Follow, Attack }
public enum MoveMode { Impulse, Walk }
