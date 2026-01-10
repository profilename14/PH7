using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
//using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

// Thomas Watson
// Enemy Behavior code


public class EnemyBehaviorStrider : EnemyBehavior
{

    [Header("STRIDER")]
    // Used by war striders. Might turn into a special behavior id later.
    public bool doubleDash = false;
    private bool dashCombo = false; // Used to tell if the second dash should happen sooner.

    // Impulse Move Type
    public float thrust = 65f;
    public float ThrustDelay = .75f;
    public float randomFactorRange = 0.1f;

    //Animation-related
    public float animDelay;

    protected bool ImpulseActive;

    protected Coroutine PursueImpulse;

    bool gotHitstunned;



    private void Awake()
    {
        CurrentHealth = StartHealth;
        CurrentPH = StartPH;
        ImpulseActive = false;
        PlayerDetector = StartCoroutine(DetectPlayer());

        enemyRigidbody = GetComponent<Rigidbody>();
    }

// Fixed Update calls move and rotate

// Update deals with stuff like ph


    protected override void Movement()
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


        NextWaypointDistance = 3;

        if (!ImpulseActive)
        {
            PursueImpulse = StartCoroutine(ImpulsePursuit());
        }

    }


    protected IEnumerator ImpulsePursuit()
    {
        ImpulseActive = true;


        while (true)
        {
            if (gotHitstunned) {
              // Striders attack after a briefer delay when hitstunned to make them a bit less weak.
              yield return new WaitForSeconds(ThrustDelay + Random.Range(-randomFactorRange, randomFactorRange));
            } else {
              yield return new WaitForSeconds(ThrustDelay + Random.Range(-randomFactorRange, randomFactorRange));
            }

            if (hitStunTimer > 0) {
              hitStunTimer = 0; // You have to hit after they begin animating to hitstun.
            }
            gotHitstunned = false;

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

                    // For hitstun to be responsive:
                    bool stop = false;
                    vulnerabilityTimer = vulnerabilityTime;
                    for (int i = 0; i < 10; i++) {
                      yield return new WaitForSeconds(animDelay / 10);
                      if (hitStunTimer > 0) {
                        //anim.ResetTrigger("Charge");
                        anim.Play("Charge", 0, 1);
                        //anim.SetBool("Idle", true);
                        vulnerabilityTimer = 0;
                        gotHitstunned = true;
                        stop = true;
                        break;
                      }
                    }
                    if (stop) {
                      continue;
                    }

                    enemyRigidbody.AddForce(velocity, ForceMode.Impulse);

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


}

// States
