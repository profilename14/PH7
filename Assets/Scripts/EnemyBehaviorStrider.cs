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



    private void Awake()
    {
        CurrentHealth = StartHealth;
        CurrentPH = StartPH;
        ImpulseActive = false;
        PlayerDetector = StartCoroutine(DetectPlayer());
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


}

// States
