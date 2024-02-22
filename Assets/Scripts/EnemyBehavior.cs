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
    public float CurrentPH;
    public float RegenPH = 0.33f; // This much H regened toward default per second.
    protected float RegenPHTimer = 0.0f;
    protected float RegenPHCooldown = 2.0f; // How long after a pH attack regen is disabled
    protected bool isExtendedClass = false; // Used by anything inheriting EnemyBehavior.

    [SerializeField]
    public PHDefaultType phDefaultType;
    public float pHMin;
    public float pHMax;
    public float pHResistFactor = 1;

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
    public float currentWalkSpeed;
    public float walkingTime = 5;
    public float currentWalkingTime;
    public float randomFactor = 0.5f;
    public float pauseTime = 2;
    public float currentPauseTime;
    private float timer;
    private bool isPaused = false;
    private float randomNum;

    // Impulse Move Type
    public float thrust = 30f;
    public float currentThrust;
    public float ThrustDelay = .75f;
    public float currentThrustDelay;
    public float randomFactorRange = 0.1f;

    //Animation-related
    public float animDelay;
    public float currentAnimDelay;
    public Animator anim;

    // Pathfinding
    public float NextWaypointDistance = 3;
    protected bool ReachedPathEnd;
    protected Path path;
    protected int CurrentWaypoint = 0;
    public State CurrentState = State.Idle;
    protected Vector3 LastKnownPos;
    protected Seeker seeker;
    protected bool ImpulseActive;

    [Header("ATTACKS")]
    protected Coroutine PlayerDetector;
    protected Coroutine PursueImpulse;

    [SerializeField] private Transform PopupPrefab;

    public DamagePlayer damageHitboxScript;

    //0.27x for fully neutralized, 1.69x for fully acidic/basic.
    public float neutralizationFactor;

    private bool disableRotation = false;

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
        randomNum = Random.Range(-randomFactor, randomFactor);
        CurrentHealth = StartHealth;
        CurrentPH = StartPH;
        ImpulseActive = false;
        PlayerDetector = StartCoroutine(DetectPlayer());

        if (StartPH < 7)
        {
            phDefaultType = PHDefaultType.Acidic;
            pHMax = 7;
            pHMin = 0;
        }
        else if (StartPH == 7)
        {
            phDefaultType = PHDefaultType.Neutral;
            pHMax = 7;
            pHMin = 7;
        }
        else if (StartPH > 7)
        {
            phDefaultType = PHDefaultType.Alkaline;
            pHMax = 14;
            pHMin = 7;
        }
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
        if(MovementMode == MoveMode.Walk) timer += Time.deltaTime;

        if (MovementMode == MoveMode.Walk && CurrentState == State.Idle)
        {
            anim.SetBool("Walking", false);
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

        if (phDefaultType == PHDefaultType.Alkaline) neutralizationFactor = Mathf.Pow(1.1f, CurrentPH - 12);
        else if (phDefaultType == PHDefaultType.Acidic) neutralizationFactor = Mathf.Pow(1.1f, 2 - CurrentPH);

        currentThrust = thrust * neutralizationFactor;
        currentThrustDelay = ThrustDelay / neutralizationFactor;
        currentWalkSpeed = WalkSpeed * neutralizationFactor;
        currentWalkingTime = walkingTime / neutralizationFactor;
        currentPauseTime = pauseTime * neutralizationFactor;

        anim.speed = neutralizationFactor;
        currentAnimDelay = animDelay / neutralizationFactor;

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

    protected void Rotation()
    {
        if (path == null || disableRotation) { // spam preventer
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
                if(isPaused && timer > currentPauseTime + randomNum)
                {
                    randomNum = Random.Range(-randomFactor, randomFactor);
                    isPaused = false;
                    timer = 0;
                }
                else if(!isPaused && timer > currentWalkingTime + randomNum)
                {
                    randomNum = Random.Range(-randomFactor, randomFactor);
                    isPaused = true;
                    timer = 0;
                }

                NextWaypointDistance = 1;
                if (!isExtendedClass) {
                    if (!isPaused) anim.SetBool("Walking", true);
                    else anim.SetBool("Walking", false);
                }
                if (ImpulseActive)
                {
                    StopCoroutine(PursueImpulse);
                }
                if (path != null) { // preventing spam
                    if (!isPaused)
                    {
                        if (movesInRotationDir)
                        {
                            GetComponent<Rigidbody>().AddForce((transform.forward).normalized * currentWalkSpeed);
                        }
                        else
                        {
                            GetComponent<Rigidbody>().AddForce((path.vectorPath[CurrentWaypoint] - transform.position).normalized * currentWalkSpeed);
                        }
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

    public void TakeDamage(float damage, float attackPh, float ph, float knockback, Vector3 sourcePos)
    {

        ChangePH(ph);

        if (CurrentPH > 14) {
          CurrentPH = 14;
        } else if (CurrentPH < 0) {
          CurrentPH = 0;
        }

        // pH formula: (1 + 0.057 * x^1.496) times damage
        /*float pHDifference = Mathf.Abs(StartPH - CurrentPH);
        float multiplier = 1 + 0.057f * Mathf.Pow(pHDifference, 1.496f);
        CurrentHealth -= damage * multiplier;
        float displayedMultiplier = Mathf.Round(multiplier * 10.0f) * 0.1f; // Rounded to 1 decimal*/

        //Degree of how much armor an enemy has. At 7, min value is ~0.28, increasing damage by ~3.7x.
        //At max/min pH for that type, divides incoming damage by the max value of ~1.7.
        float armorFactor = 1;

        if(phDefaultType == PHDefaultType.Alkaline) armorFactor = Mathf.Pow(1.3f, CurrentPH - 12);
        else if (phDefaultType == PHDefaultType.Acidic) armorFactor = Mathf.Pow(1.3f, 2 - CurrentPH);

        //The player's offensive pH multiplier. If max (player pH is at 0/14 and is alkaline/acidic enemy), mult is 3.58x damage.
        float phMultiplier = 1;

        if(phDefaultType == PHDefaultType.Alkaline && attackPh < 7)
        {
            phMultiplier = Mathf.Pow(1.2f, 7 - attackPh);
        }
        else if (phDefaultType == PHDefaultType.Acidic && attackPh > 7)
        {
            phMultiplier = Mathf.Pow(1.2f, attackPh - 7);
        }

        float totalDamage = (damage * phMultiplier) / armorFactor;

        CurrentHealth -= totalDamage;

        RegenPHTimer = RegenPHCooldown;

        if (damage > 0) {
            Debug.Log(this.gameObject.name + " took damage: " + totalDamage + " with armor factor: " + armorFactor + " and pH multiplier: " + phMultiplier);
        }

        // Damage Text Popup
        /*Transform PopupTransform = Instantiate(PopupPrefab, transform.position, Quaternion.identity);
        DamagePopup popup = PopupTransform.GetComponent<DamagePopup>();
        popup.Setup(damage);*/
        

        // Knockback
        Vector3 dir = -((sourcePos - transform.position).normalized);
        Vector3 velocity = dir * knockback;
        GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);

        if (CurrentHealth <= 0) Destroy(this.gameObject);
    }

    public void Neutralize(float amount)
    {
        //Debug.Log("Neutralize spell");
        if(phDefaultType == PHDefaultType.Acidic)
        {
            ChangePH(amount);
        }
        else if (phDefaultType == PHDefaultType.Alkaline)
        {
            ChangePH(-amount);
        }
    }

    public void ChangePH(float amount)
    {
        CurrentPH = Mathf.Clamp(CurrentPH + (amount / pHResistFactor), pHMin, pHMax);
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
            yield return new WaitForSeconds(currentThrustDelay + Random.Range(-randomFactorRange, randomFactorRange));
            float angle = 10;
            if (path == null) {
              // Lets just wait for now.to avoid spam
            }
            else if (Vector3.Angle(transform.forward, (path.vectorPath[CurrentWaypoint] - transform.position)) < angle)
            {
                if (!ReachedPathEnd)
                {
                    disableRotation = true;
                    Vector3 dir = (path.vectorPath[CurrentWaypoint] - transform.position).normalized;
                    Vector3 velocity = dir * currentThrust;

                    anim.SetTrigger("Charge");
                    yield return new WaitForSeconds(currentAnimDelay);
                    GetComponent<Rigidbody>().velocity = new Vector2(0, 0);
                    GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);
                    //Debug.Log("Strider added force: " + velocity);

                    if (doubleDash) { // If we're a war strider
                      if (dashCombo == false) { // If we did an initial dash
                        currentThrustDelay = ThrustDelay / 1.5f;
                        TurnRate = TurnRate * 2.5f;
                        dashCombo = true;
                      } else { // If we're wrapping up our second dash.
                        currentThrustDelay = ThrustDelay * 1.5f;
                        TurnRate = TurnRate / 2.5f;
                        dashCombo = false;
                      }
                    }
                    disableRotation = false;
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
            /*if (other.gameObject.GetComponent<MovementController>().isDashing) {
              if (!other.gameObject.GetComponent<MovementController>().dashEnding)
              {
                TakeDamage(3, 1.5f, 7, other.gameObject.transform.position);
              }
              else
              {
                TakeDamage(2, 1f, 3.5f, other.gameObject.transform.position);
              }
            }*/

        }
    }

    public void AlertEnemy() { // To be called by any trigger for a door the enemy is guarding.
        CurrentState = State.Follow;
    }
}

// States
public enum State { Inactive, Idle, Seek, Follow, Attack }
public enum MoveMode { Impulse, Walk }

public enum PHDefaultType {Acidic, Neutral, Alkaline}
