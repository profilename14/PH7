using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Patterns;

public class ScuttlerAI : EnemyAI
{
    [Header("State Transition Variables")]
    public float timeToRedecideState;

    /*[Header("Throwable")]
    public Throwable throwableScript;*/

    [Header("Next State Info")]
    public string nextChosenState;
    private float nextChosenAttackRange;
    public float timeToAttackNext;
    private float attackTimer;
    private float redecideStateTimer;

    [Header("Left Claw Attack")]
    public float leftClawAttackRange;
    public float leftClawAdditionalDelay;
    public float leftClawForwardForce;

    [Header("Randomized Delay Range")]
    public float minTimeToAttack = 0.5f; // Min time after reaching chosen attack range to begin attack
    public float maxTimeToAttack = 2f; // Max time after reaching chosen attack range to begin attack

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        fsm.Add("Left Claw", new EnemyState(fsm, "Left Claw", this));
        fsm.Add("Bubbled", new EnemyState(fsm, "Bubbled", this));
        Init_Follow();
        Init_Left_Claw();
        Init_Bubbled();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    private void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void Init_Follow()
    {
        EnemyState state = (EnemyState)fsm.GetState("Follow");

        state.OnEnterDelegate += delegate ()
        {
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
            nextChosenState = "Left Claw";
            nextChosenAttackRange = leftClawAttackRange;
            timeToAttackNext = Random.Range(minTimeToAttack, maxTimeToAttack);
        };

        state.OnUpdateDelegate += delegate ()
        {
            if (Vector3.Distance(this.transform.position, player.transform.position) < nextChosenAttackRange)
            {
                attackTimer += Time.deltaTime;
            }
            /*if (throwableScript.isBeingCarried)
            {
                fsm.SetCurrentState("Bubbled");
            }*/
            if (attackTimer > timeToAttackNext)
            {
                fsm.SetCurrentState(nextChosenState);
            }

            if (Vector3.Distance(this.transform.position, player.transform.position) <= followDistance)
            {
                if (!isCircling)
                {
                    if (Random.Range(0, 1f) > 0.5) isCirclingRight = true;
                    else isCirclingRight = false;
                }

                isCircling = true;
            }
            else
            {
                isCircling = false;
            }
        };
    }

    void Init_Left_Claw()
    {
        EnemyState state = (EnemyState)fsm.GetState("Left Claw");

        state.OnEnterDelegate += delegate ()
        {
            ai.isStopped = true;
            target.transform.position = player.transform.position;
            anim.SetTrigger("Left Claw");
        };

        state.OnUpdateDelegate += delegate ()
        {
            target.transform.position = player.transform.position;
            var direction = (target.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 1.25f);
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
        if(state == "Left Claw")
        {
            GetComponent<Rigidbody>().AddForce(transform.forward * leftClawForwardForce, ForceMode.Impulse);
            ai.enableRotation = false;
        }
    }

    public void EndAttack()
    {
        inInterruptFrames = true;
        fsm.SetCurrentState("Follow");
    }
}
