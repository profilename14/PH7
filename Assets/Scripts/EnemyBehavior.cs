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
    [HideInInspector] public float StartPH; // Set in code. Enemies are 0, 7, or 14 to keep things simple
    [HideInInspector] public float CurrentPH;
    protected float RegenPH = 0.0f; // Set in code
    //protected float RegenPHTimer = 0.0f;
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

    // Used by war striders. Might turn into a special behavior id later.
    public bool doubleDash = false;
    private bool dashCombo = false; // Used to tell if the second dash should happen sooner.
    protected bool movesInRotationDir = false; // used by hitbox attackers to move with attacks.

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
    protected bool ReachedPathEnd;
    protected Path path;
    protected int CurrentWaypoint = 0;
    protected State CurrentState = State.Idle;
    protected Vector3 LastKnownPos;
    protected Seeker seeker;
    protected bool ImpulseActive;

    [Header("ATTACKS")]
    protected Coroutine PlayerDetector;
    protected Coroutine PursueImpulse;

    public TypesPH naturalPH;

    [HideInInspector] public float stunRecoveryTimer = 0; // Was stunned earlier OR is currently stunned, cannot change pH. Is stunTime + recoveryTime
    [SerializeField]  protected float stunRecoveryTime = 4.0f; // Time after stun when pH can't be changed. High for alkaline
    [HideInInspector] public float stunTimer = 0; // Is currently stunned, taking bonus damage and incapacited
    [SerializeField]  protected float stunTime = 2.0f;
    protected float bonusDamageInRecovery = 2.0f; // Should be kept the same between enemies
    protected float bonusDamageInStun = 4.0f;

    public float damageMultToPH = 1; // Multiplier for how much pH spells affect the enemy. 1x for most



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
        ImpulseActive = false;
        PlayerDetector = StartCoroutine(DetectPlayer());

        if (StartPH > 7) {
          naturalPH = TypesPH.Alkaline;
          StartPH = 14;
        } else if (StartPH < 7) {
          naturalPH = TypesPH.Acidic;
          StartPH = 0;
        } else {
          //naturalPH = TypesPH.Neutral;
        }

        CurrentPH = StartPH;

        RegenPH = 7.0f / stunRecoveryTime; // takes exactly recovery time to regen 7
    }

    // We may want a "Favorite Room" or "Default Position" so that enemies know where to return to if they lose track of a player.
    // That, or they just return to a default idle where they choose a random nearby location and patrol around it.

    void FixedUpdate() {
      if (CurrentState != State.Idle && CurrentState != State.Stunned) { // If the enemy hasn't seen the player
        Movement();
        Rotation();
      }
    }

    void Update()
    {
        if(MovementMode == MoveMode.Walk && (CurrentState == State.Idle || CurrentState == State.Stunned))
        {
            anim.SetBool("Walking", false);
        }

        if (stunTimer > 0) {
          stunTimer -= Time.deltaTime;

          if (stunTimer <= 0) {
            CurrentState = State.Follow;

            if (CurrentPH < StartPH) {
              CurrentPH += 0.1f;
            } else if (CurrentPH > StartPH) {
              CurrentPH -= 0.1f;
            }
          }
        }

        if (stunRecoveryTimer > 0) {
          stunRecoveryTimer -= Time.deltaTime;
        }

        if (CurrentState == State.Stunned) {
          // Nothing happens
        } else if (stunRecoveryTimer > 0) {
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

            case State.Stunned:
                //Debug.Log("Stunned!");

                break;
        }
    }

    protected void Rotation()
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
    protected void Movement()
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
                if (!isExtendedClass) {
                  anim.SetBool("Walking", true);
                }
                if (ImpulseActive)
                {
                    StopCoroutine(PursueImpulse);
                }
                if (path != null) { // preventing spam
                  if (movesInRotationDir) {
                    GetComponent<Rigidbody>().AddForce((transform.forward).normalized * WalkSpeed);
                  } else {
                    GetComponent<Rigidbody>().AddForce((path.vectorPath[CurrentWaypoint] - transform.position).normalized * WalkSpeed);
                  }
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

        if (stunRecoveryTimer > 0) {
          ph = 0;
        }

        if (naturalPH == TypesPH.Alkaline) {
          CurrentPH += ph * damageMultToPH;
          if (CurrentPH <= 7 && CurrentState != State.Stunned) {
            StunEnemy();
          }

        } else if (naturalPH == TypesPH.Acidic) {
          CurrentPH += ph * damageMultToPH;
          if (7 <= CurrentPH && CurrentState != State.Stunned) {
            StunEnemy();
          }

        } else {
          // Neutral enemies take a little damage so spells aren't useless against them.
          // They have projectiles so its not like you can spam them down or anything.
          damage += Mathf.Abs(ph) * 2 * damageMultToPH;
        }

        if (CurrentPH > 14) {
          CurrentPH = 14;
        } else if (CurrentPH < 0) {
          CurrentPH = 0;
        }

        // pH formula: (1 + 0.057 * x^1.496) times damage
        //float pHDifference = Mathf.Abs(StartPH - CurrentPH);
        //float multiplier = 1 + 0.057f * Mathf.Pow(pHDifference, 1.496f);
        if (CurrentState == State.Stunned) {
          damage = damage * 4;
        } else if (stunRecoveryTimer > 0) {
          damage = damage * 2;
        } else {
          float distanceFromSeven = Mathf.Abs(CurrentPH - 7);
          damage = damage * 2 - damage * (distanceFromSeven/7);
        }

        CurrentHealth -= damage;

        float displayedDamage = Mathf.Round(damage * 10.0f) * 0.1f; // Rounded to 1 decimal

        if (ph != 0) {
          //RegenPHTimer = RegenPHCooldown;
        }

        if (damage > 0) {
          Debug.Log("Damage: " + displayedDamage);

        }


        Vector3 dir = -((sourcePos - transform.position).normalized);
        Vector3 velocity = dir * knockback;
        GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);

        if (CurrentHealth <= 0) Destroy(this.gameObject);
    }


    protected IEnumerator DetectPlayer()
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

    protected IEnumerator ImpulsePursuit()
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
        if (CurrentState != State.Stunned) {
          CurrentState = State.Follow;
        }
    }

    void StunEnemy() {
      CurrentPH = 7;
      CurrentState = State.Stunned;
      stunRecoveryTimer = stunRecoveryTime + stunTime;
      stunTimer = stunTime;


    }
}

// States
public enum State { Inactive, Idle, Seek, Follow, Attack, Stunned }
public enum MoveMode { Impulse, Walk }
public enum TypesPH { Alkaline, Neutral, Acidic }
