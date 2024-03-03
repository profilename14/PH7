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


public class EnemyBehaviorAxehead : EnemyBehavior
{

    public float attackRange = 3;
    public float attackTime = 2f;
    protected float attackTimer = 0.0f;
    public float speedMultDuringAttack = 0.5f;
    protected bool canMoveDuringAttack = true;
    public float rotationMultDuringAttack = 0.5f;
    protected bool canRotateDuringAttack = true;

    public Collider hitbox;
    public float timeUntilHitbox = 0.3f;
    public float timeAfterHitbox = 0.8f;
    private float originalRotation;
    private float originalSpeed;



    // awake and update can be redefined.
    private void Awake()
    {
        originalSpeed = WalkSpeed;
        originalRotation = TurnRate;
        CurrentHealth = StartHealth;
        CurrentPH = StartPH;
        //ImpulseActive = false;
        PlayerDetector = StartCoroutine(DetectPlayer());

        if (PlayerDetected) {
          CurrentState = State.Follow;
        }

        isExtendedClass = true;
        hitbox.enabled = false;
    }

    // We may want a "Favorite Room" or "Default Position" so that enemies know where to return to if they lose track of a player.
    // That, or they just return to a default idle where they choose a random nearby location and patrol around it.
    void FixedUpdate()
    {

      if (CurrentState == State.Follow) { // If the enemy has seen the player
        Rotation();
        Movement();

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

        //anim.SetBool("Swim Fast", false);

        if (hitStunTimer > 0) {
          hitStunTimer -= Time.deltaTime;
        }
        if (vulnerabilityTimer > 0) {
          vulnerabilityTimer -= Time.deltaTime;
        }

        if (attackTimer > 0.0f) {
          attackTimer -= Time.deltaTime;

          if (hitStunTimer > 0) {
            //anim.ResetTrigger("Attack");
            anim.Play("Attack", 0, 1); // This instantly ends the animation by skipping to 100% time
            hitbox.enabled = false;
            CurrentState = State.Follow;
          }

          if (attackTimer > attackTime - timeUntilHitbox) {
            hitbox.enabled = false;
          }
          else if (attackTimer < timeAfterHitbox) {
            hitbox.enabled = false;
          }
          else {
            hitbox.enabled = true;
          }
        } else if (attackTimer <= 0.0f && CurrentState == State.Attack) {
            CurrentState = State.Follow;
            if (canMoveDuringAttack) {
              WalkSpeed = originalSpeed;
            }
            if (canRotateDuringAttack) {
              TurnRate =originalRotation;
            }
            movesInRotationDir = false;
            hitbox.enabled = false;

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


    private void checkForAttack() {
      Vector3 distanceToTarget = target.position - transform.position;
      float distance = distanceToTarget.magnitude;

      if (hitStunTimer > 0) {
        return;
      }

      if (attackTimer <= 0 && distance < attackRange) {
        makeAttack();
      }

    }

    private void makeAttack() {

      if (attackTimer > 0) {
        return;
      }

      CurrentState = State.Attack; // lock rotation and movement

      vulnerabilityTimer = vulnerabilityTime;

      var toTarget = path.vectorPath[CurrentWaypoint] - transform.position;
      toTarget.y = 0f;

      if (toTarget != new Vector3(0,0,0)) { // Immediately look to target
          //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget), 360f); // Almost can 180
      }

      movesInRotationDir = true;

      attackTimer = attackTime;
      if (canMoveDuringAttack) {
        WalkSpeed *= speedMultDuringAttack;
      }
      if (canRotateDuringAttack) {
        TurnRate *= rotationMultDuringAttack;
      }

      //Debug.Log("Attacking! (Axehead)");


      anim.SetTrigger("Attack");


    }
}
