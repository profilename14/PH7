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
    protected float CurrentHealth;
    [Range(0f, 14f)]
    public float StartPH;
    protected float CurrentPH;
    public float RegenPH = 0.33f; // This much H regened toward default per second.
    protected float RegenPHTimer = 0.0f;
    protected float RegenPHCooldown = 2.0f; // How long after a pH attack regen is disabled
    protected bool isExtendedClass = false; // Used by anything inheriting EnemyBehavior.


    [Header("DETECTION")]
    public bool PlayerDetected = false;
    //public float DetectionRadius;
    protected float DetectionDelay = .25f;

    [Header("MOVEMENT")]
    // Reminder: Add an option for what movement mode the enemy should use
    // i.e. Impulse, regular walking, etc.
    public Transform target;

    public MoveMode MovementMode = MoveMode.Walk;
    public float TurnRate = 360f;

    protected bool movesInRotationDir = false; // used by hitbox attackers to move with attacks.

    // Walk Move Type
    public float WalkSpeed = 0.005f;


    //Animation-related
    public Animator anim;

    // Pathfinding
    public float NextWaypointDistance = 3;
    protected bool ReachedPathEnd;
    protected Path path;
    protected int CurrentWaypoint = 0;
    protected State CurrentState = State.Idle;
    protected Vector3 LastKnownPos;
    protected Seeker seeker;

    [Header("ATTACKS")]
    protected Coroutine PlayerDetector;

    [SerializeField] private Transform PopupPrefab;


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
        //ImpulseActive = false;
        PlayerDetector = StartCoroutine(DetectPlayer());
    }

    // We may want a "Favorite Room" or "Default Position" so that enemies know where to return to if they lose track of a player.
    // That, or they just return to a default idle where they choose a random nearby location and patrol around it.

    void FixedUpdate() {
      if (CurrentState != State.Idle) { // If the enemy hasn't seen the player
        Movement();
        Rotation();
      }
    }

    void Update()
    {


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

    protected virtual void Rotation()
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

    protected virtual void Movement()
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

        NextWaypointDistance = 1;
        /*if (!isExtendedClass) {
          anim.SetBool("Walking", true);
        }*/
        /*if (ImpulseActive)
        {
            StopCoroutine(PursueImpulse);
        }*/
        if (path != null) { // preventing spam
          if (movesInRotationDir) {
            GetComponent<Rigidbody>().AddForce((transform.forward).normalized * WalkSpeed);
          } else {
            GetComponent<Rigidbody>().AddForce((path.vectorPath[CurrentWaypoint] - transform.position).normalized * WalkSpeed);
          }
        }

        //GetComponent<Rigidbody>.AddForce();

    }

    public virtual void TakeDamage(float damage, float ph, float knockback, Vector3 sourcePos)
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

        // Damage Text Popup
        Transform PopupTransform = Instantiate(PopupPrefab, transform.position, Quaternion.identity);
        DamagePopup popup = PopupTransform.GetComponent<DamagePopup>();
        popup.Setup(damage);


        // Knockback
        Vector3 dir = -((sourcePos - transform.position).normalized);
        Vector3 velocity = dir * knockback;
        GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);

        if (CurrentHealth <= 0) Destroy(this.gameObject);
    }

    protected virtual IEnumerator DetectPlayer()
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
            if (gameObject.GetComponent<MovementController>() != null) {
              // If its the old movement controller
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
            } else {
              // If its the instant movement controller
              if (other.gameObject.GetComponent<IIMovementController>().isDashing) {
                if (!other.gameObject.GetComponent<IIMovementController>().dashEnding)
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
    }

    public void AlertEnemy() { // To be called by any trigger for a door the enemy is guarding.
        CurrentState = State.Follow;
    }
}

// States
public enum State { Inactive, Idle, Seek, Follow, Attack }
public enum MoveMode { Impulse, Walk }
