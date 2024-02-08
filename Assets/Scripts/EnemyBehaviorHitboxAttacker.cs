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
    protected float attackTimer = 0.0f;
    protected float speedMultDuringAttack = 0.5f;
    protected bool canMoveDuringAttack = true;


    // awake and update can be redefined.
    private void Awake()
    {
        CurrentHealth = StartHealth;
        CurrentPH = StartPH;
        ImpulseActive = false;
        PlayerDetector = StartCoroutine(DetectPlayer());

        if (PlayerDetected) {
          CurrentState = State.Follow;
        }
    }

    // We may want a "Favorite Room" or "Default Position" so that enemies know where to return to if they lose track of a player.
    // That, or they just return to a default idle where they choose a random nearby location and patrol around it.
    void FixedUpdate()
    {
      if (CurrentState == State.Follow) { // If the enemy hasn't seen the player
        Rotation();
        Movement();
      } else if (CurrentState == State.Attack) {
        if (canMoveDuringAttack) {
          Movement();
        }

      }
    }


    void Update()
    {


        if (attackTimer > 0.0f) {
          attackTimer -= Time.deltaTime;
        } else if (attackTimer <= 0.0f && CurrentState == State.Attack) {
            CurrentState = State.Follow;
            if (canMoveDuringAttack) {
              WalkSpeed /= speedMultDuringAttack;
            }

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

    private void checkForAttack() {
      Vector3 distanceToTarget = target.position - transform.position;
      float distance = distanceToTarget.magnitude;

      if (attackTimer <= 0 && distance < attackRange) {
        makeAttack();
      }



    }

    private void makeAttack() {

      CurrentState = State.Attack; // lock rotation and movement

      var toTarget = path.vectorPath[CurrentWaypoint] - transform.position;
      toTarget.y = 0f;

      if (toTarget != new Vector3(0,0,0)) { // Immediately look to target
          transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget), 155f); // Almost can 180
      }

      if (attackTimer > 0) {
        return;
      }

      attackTimer = attackTime;
      if (canMoveDuringAttack) {
        WalkSpeed *= speedMultDuringAttack;
      }


      Debug.Log("Attacking!");

    }
}
