using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Patterns;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

public class WarStriderAI : EnemyAI
{
    [Header("State Transition Variables")]
    public float timeToRedecideState;

    /*[Header("Throwable")]
    public Throwable throwableScript;*/

    [Header("Next State Info")]
    public string nextChosenState;
    private float nextChosenAttackRange;
    public float timeToAttackNext;
    public float attackTimer;
    private float redecideStateTimer;

    [Header("Charge Attack")]
    public float chargeAttackRange;
    public float chargeAdditionalDelay;
    public float chargeForwardForce;
    public float dashForce;

    
    [Header("Spit Attack")]
    public GameObject venomProjectilePrefab;

    [Header("Randomized Delay Range")]
    public float minTimeToAttack = 0.5f; // Min time after reaching chosen attack range to begin attack
    public float maxTimeToAttack = 2f; // Max time after reaching chosen attack range to begin attack
    public bool isFlying = true;
    public float groundedDashProbability = 0.75f;
    public float timeToRecoverFlight = 10f;
    private float recoveryTimer = 0;

 
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
       
        fsm.Add("Fall", new EnemyState(fsm, "Fall", this));
        fsm.Add("Spit", new EnemyState(fsm, "Spit", this));
        fsm.Add("Charge", new EnemyState(fsm, "Charge", this));
        fsm.Add("Bubbled", new EnemyState(fsm, "Bubbled", this));
        Init_Follow();
        Init_Fall();
        Init_Spit();
        Init_Charge();
        Init_Bubbled();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if (isFlying == false) {
            recoveryTimer += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        base.FixedUpdate();
    }


    void Init_Fall()
    {
        EnemyState state = (EnemyState)fsm.GetState("Fall");

        state.OnEnterDelegate += delegate ()
        {

        };

        state.OnUpdateDelegate += delegate ()
        {
            //if(Vector3.Distance(this.transform.position, player.transform.position) < nextChosenAttackRange)
            //attackTimer += Time.deltaTime;

            /*if (throwableScript.isBeingCarried)
            {
                fsm.SetCurrentState("Bubbled");
            }*/
        };


    }

    void Init_Spit()
    {
        EnemyState state = (EnemyState)fsm.GetState("Spit");

        state.OnEnterDelegate += delegate ()
        {
            ai.isStopped = true;
            ai.enableRotation = true;
            anim.SetTrigger("Spit");
        };

        state.OnUpdateDelegate += delegate ()
        {
            //if(Vector3.Distance(this.transform.position, player.transform.position) < nextChosenAttackRange)
            //attackTimer += Time.deltaTime;

            /*if (throwableScript.isBeingCarried)
            {
                fsm.SetCurrentState("Bubbled");
            }*/
            if (ai.enableRotation) {
                target.transform.position = player.transform.position;
                var direction = (target.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
            }
        };

        state.OnExitDelegate += delegate ()
        {
            ai.isStopped = false;
            ai.enableRotation = true;
        };


    }



    void Init_Follow()
    {
        EnemyState state = (EnemyState)fsm.GetState("Follow");

        state.OnEnterDelegate += delegate ()
        {
            if (isFlying == true) { // For idle sending us to follow
                //fsm.SetCurrentState("Fly");
            }
            if (canBeHitstunned == false) {
                canBeHitstunned = true;
            }
            if (wasHitstunned == false) {
                attackTimer = 0;
            } else {
                attackTimer += 0.25f;
                wasHitstunned = false;
                if (attackTimer >= maxTimeToAttack * 1.0f) {
                    canBeHitstunned = false;
                }
            }
            redecideStateTimer = 0;
            if (isFlying == true) { // For idle sending us to follow
                nextChosenState = "Spit";
            } else {
                float num = Random.value;
                if (num < groundedDashProbability) {
                    nextChosenState = "Charge";
                } else {
                    if (recoveryTimer > timeToRecoverFlight) {
                        recoveryTimer = 0;
                        armor = 1;
                        armorBroken = false;
                        isFlying = true;
                        nextChosenState = "Spit";
                        anim.SetBool("IsFlying", true);
                        fsm.SetCurrentState("Fall"); // Blanks state for waiting
                    } else {
                        nextChosenState = "Spit";
                    }
                    
                }
                
            }
            
            nextChosenAttackRange = chargeAttackRange;
            timeToAttackNext = Random.Range(minTimeToAttack, maxTimeToAttack) + chargeAdditionalDelay;
            //anim.SetTrigger("Follow");
        };

        state.OnUpdateDelegate += delegate ()
        {
            //if(Vector3.Distance(this.transform.position, player.transform.position) < nextChosenAttackRange)
            attackTimer += Time.deltaTime;

            /*if (throwableScript.isBeingCarried)
            {
                fsm.SetCurrentState("Bubbled");
            }*/
            if (attackTimer > timeToAttackNext)
            {
                fsm.SetCurrentState(nextChosenState);
            }

            if (armor <= 0 && anim.GetBool("IsFlying"))
            {
                isFlying = false;
                anim.SetBool("IsFlying", false);
                fsm.SetCurrentState("Fall");

                //anim.SetTrigger("Fall");
            }

            if (isFlying) {
                isCircling = true;
            } else {
                isCircling = false;
            }
        };
    }

    void Init_Charge()
    {
        EnemyState state = (EnemyState)fsm.GetState("Charge");

        state.OnEnterDelegate += delegate ()
        {
            ai.isStopped = true;
            ai.enableRotation = true;
            target.transform.position = player.transform.position;
            anim.SetTrigger("Charge");
        };
        state.OnUpdateDelegate += delegate ()
        {
            target.transform.position = player.transform.position;
            if (ai.enableRotation) {
                var direction = (target.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2); // 2.5x  slower
            }
            
        };

        state.OnExitDelegate += delegate ()
        {
            ai.isStopped = false;
            ai.enableRotation = true;
        };
    }

    void Init_Bubbled()
    {
        EnemyState state = (EnemyState)fsm.GetState("Bubbled");

        state.OnEnterDelegate += delegate ()
        {
            ai.isStopped = true;
            ai.enableRotation = false;
            anim.SetBool("Bubbled", true);
        };

        state.OnExitDelegate += delegate ()
        {
            ai.isStopped = false;
            ai.enableRotation = true;
            anim.SetBool("Bubbled", false);
        };
    }

    public void StartAttack(string state)
    {
        if(state == "Charge")
        {
            ai.enableRotation = true;
            ai.isStopped = true;
            DashImpulse();
        }
        else if(state == "Spit")
        {
            ai.enableRotation = true;
            ai.isStopped = true;
            SpitVenom();
        }
    }

    public void DashImpulse()
    {
        ai.enableRotation = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce((transform.forward).normalized * dashForce, ForceMode.Impulse);
    }

    public void SpitVenom()
    {
        Vector3 SpitLocation = transform.position + transform.forward + new Vector3(0, 0.9f, 0);
        Vector3 curRotation = transform.forward;
        float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

        Instantiate(venomProjectilePrefab, SpitLocation, Quaternion.Euler(0, angle, 0) );

        ai.enableRotation = false;
    }

    public void DashStop()
    {
        StopAllCoroutines();
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        ai.isStopped = true;
        ai.enableRotation = true;
        GetComponent<Rigidbody>().AddForce(transform.forward.normalized * chargeForwardForce, ForceMode.Impulse);
    }

    public void EndAttack()
    {
        inInterruptFrames = true;
        fsm.SetCurrentState("Follow");
    }

    public void FinishedFalling()
    {
        fsm.SetCurrentState("Follow");
    }

    public void FinishedTakingOff()
    {
        fsm.SetCurrentState("Follow");
    }

    
}
