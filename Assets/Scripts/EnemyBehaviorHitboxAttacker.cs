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


public class EnemyBehaviorHitboxAttacker : EnemyBehavior
{

    public float attackRange = 3;
    public float attackTime = 2f;
    public float currentAttackTime;
    protected float attackTimer = 0.0f;
    public float speedMultDuringAttack = 0.5f;
    protected bool canMoveDuringAttack = true;
    public float rotationMultDuringAttack = 0.5f;
    protected bool canRotateDuringAttack = true;

    public bool canJump = false;  // ONLY for Vitriclaws
    public float jumpSpeed = 45f;
    public float jumpMaxTime = 1f;
    protected float jumpTimer = 0f;
    public float jumpCooldown = 7.5f;
    public float jumpDistRequirement = 10f;
    protected float jumpCooldownTimer = 0f;
    public Collider hitbox;
    public float timeUntilHitbox = 0.3f;
    public float timeAfterHitbox = 0.8f;
    private float originalRotation;
    private float originalSpeed;

    bool attacking;

    // awake and update can be redefined.
    private void Awake()
    {
        originalSpeed = WalkSpeed;
        originalRotation = TurnRate;
        CurrentHealth = StartHealth;
        CurrentPH = StartPH;
        ImpulseActive = false;
        PlayerDetector = StartCoroutine(DetectPlayer());

        if (PlayerDetected) {
          CurrentState = State.Follow;
        }

        isExtendedClass = true;
        //hitbox.enabled = false;

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
    void FixedUpdate()
    {
      if (jumpTimer > 0) {
        jumpTimer -= Time.deltaTime;
        return;
      }
      if (jumpCooldownTimer > 0) {
        jumpCooldownTimer -= Time.deltaTime;
      }

      if (CurrentState == State.Follow) { // If the enemy has seen the player
        Rotation();
        Movement();

        if(canJump == false)
        {
            anim.SetBool("Swim Fast", true);
        }
        if(canJump == true)
        {
            anim.SetBool("Walking", true);
        }

      } else if (CurrentState == State.Attack) {
        if (canMoveDuringAttack) {
          Movement();
        }
        if (canRotateDuringAttack) {
          Rotation();
        }
      }
    }


    void Update()
    {

        if (phDefaultType == PHDefaultType.Alkaline) neutralizationFactor = Mathf.Pow(1.1f, CurrentPH - 12);
        else if (phDefaultType == PHDefaultType.Acidic) neutralizationFactor = Mathf.Pow(1.1f, 2 - CurrentPH);

        currentThrust = thrust * neutralizationFactor;
        currentThrustDelay = ThrustDelay / neutralizationFactor;
        if (!attacking) currentWalkSpeed = WalkSpeed * neutralizationFactor;
        else currentWalkSpeed = WalkSpeed * neutralizationFactor * speedMultDuringAttack;
        currentWalkingTime = walkingTime / neutralizationFactor;
        currentPauseTime = pauseTime * neutralizationFactor;
        currentAttackTime = attackTime / neutralizationFactor;

        anim.speed = neutralizationFactor;
        currentAnimDelay = animDelay / neutralizationFactor;

        if (canJump == false && CurrentState == State.Idle)
      {
          anim.SetBool("Swim Fast", false);
      }
      if(canJump == true && CurrentState == State.Idle)
      {
          anim.SetBool("Walking", false);
      }


        if (attackTimer > 0.0f) {
          attackTimer -= Time.deltaTime;
          if (attackTimer > currentAttackTime - timeUntilHitbox) {
            //hitbox.enabled = false;
          }
          else if (attackTimer < timeAfterHitbox) {
            //hitbox.enabled = false;
          }
          else {
            //hitbox.enabled = true;
          }
        } else if (attackTimer <= 0.0f && CurrentState == State.Attack) {
            attacking = false;
            CurrentState = State.Follow;
            /*if (canMoveDuringAttack) {
              WalkSpeed = originalSpeed;
            }
            if (canRotateDuringAttack) {
              TurnRate =originalRotation;
            }*/
            movesInRotationDir = false;
            //hitbox.enabled = false;

        }




        if (CurrentState != State.Idle && CurrentState != State.Attack) {
          checkForAttack();
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

    private void checkForAttack() {
      Vector3 distanceToTarget = target.position - transform.position;
      float distance = distanceToTarget.magnitude;

      if (attackTimer <= 0 && distance < attackRange && jumpTimer <= 0) {
        makeAttack();
      }

      if (canJump && jumpCooldownTimer <= 0 && distance > jumpDistRequirement) {

        if (distanceToTarget != new Vector3(0,0,0)) { // Immediately look to target
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(distanceToTarget), 360f); // Almost can 180
        }

        Debug.Log("Jumping!");

        //anim.SetTrigger("Jump"); Takes like 2 seconds for the animation to actually jump, it can wait.

        jumpCooldownTimer = jumpCooldown;
        jumpTimer = jumpMaxTime;
        GetComponent<Rigidbody>().AddForce((transform.forward).normalized * jumpSpeed, ForceMode.Impulse);


      }



    }

    private void makeAttack() {
        if (attackTimer > 0 || jumpTimer > 0) {
          return;
        }
        attacking = true;
        CurrentState = State.Attack; // lock rotation and movement

        var toTarget = path.vectorPath[CurrentWaypoint] - transform.position;
        toTarget.y = 0f;

        if (toTarget != new Vector3(0,0,0)) { // Immediately look to target
          //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget), 360f); // Almost can 180
        }

        movesInRotationDir = true;

        attackTimer = currentAttackTime;
        /*if (canMoveDuringAttack) {
            WalkSpeed *= speedMultDuringAttack;
        }
        if (canRotateDuringAttack) {
            TurnRate *= rotationMultDuringAttack;
        }*/

        Debug.Log("Attacking!");


        if(canJump == false)
        {
            anim.SetTrigger("Attack");
        }
        if(canJump == true)
        {
            anim.SetTrigger("Attack");
        }


    }
}
