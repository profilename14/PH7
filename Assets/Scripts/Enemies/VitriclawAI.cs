using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Patterns;

public class VitriclawAI : EnemyAI
{
    //public float followToCirclingProbability;
    public float followToLeftClawProbability;
    public float followToRightClawProbability;
    public float followToJumpProbability;

    //public float closeCirclingDistance;

    [Header("Next State Info")]
    public string nextChosenState;
    public bool nextStateIsAttack;
    public float nextChosenAttackRange;
    public float timeToAttackNext;
    public float attackTimer;

    [Header("Left Claw Attack")]
    public float leftClawAttackRange;
    public float leftClawAdditionalDelay;
    public float leftClawForwardForce;

    public float rightClawAttackRange;
    public float rightClawAdditionalDelay;
    public float jumpAttackRange;
    public float jumpAdditionalDelay;

    [Header("Randomized Delay Range")]
    public float minTimeToAttack = 0.5f; // Min time after reaching chosen attack range to begin attack
    public float maxTimeToAttack = 2f; // Max time after reaching chosen attack range to begin attack

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        fsm.Add("Left Claw", new EnemyState(fsm, "Left Claw", this));
        fsm.Add("Right Claw", new EnemyState(fsm, "Left Claw", this));
        fsm.Add("Jump", new EnemyState(fsm, "Left Claw", this));
        Init_Follow();
        Init_Left_Claw();
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
            attackTimer = 0;

            float num = Random.value;

            if(num <= followToLeftClawProbability)
            {
                nextChosenState = "Left Claw";
                nextStateIsAttack = true;
                nextChosenAttackRange = leftClawAttackRange;
                timeToAttackNext = Random.Range(minTimeToAttack, maxTimeToAttack) + leftClawAdditionalDelay;
            }
        };

        state.OnUpdateDelegate += delegate ()
        {
            if(Vector3.Distance(this.transform.position, player.transform.position) < nextChosenAttackRange)
            {
                attackTimer += Time.deltaTime;

                //Debug.Log("In attack range!");

                if(attackTimer > timeToAttackNext)
                {
                    fsm.SetCurrentState(nextChosenState);
                }
            }
        };
    }

    void Init_Left_Claw()
    {
        EnemyState state = (EnemyState)fsm.GetState("Left Claw");

        state.OnEnterDelegate += delegate ()
        {
            //Debug.Log("OnEnter - Left Claw");
            ai.isStopped = true;
            target.transform.position = player.transform.position;
            anim.SetTrigger("Left Claw");
        };

        state.OnExitDelegate += delegate ()
        {
            //Debug.Log("OnExit - Left Claw");
            ai.isStopped = false;
            ai.enableRotation = true;
        };
    }

    public void StartAttack(string state)
    {
        if(state == "Left Claw")
        {
            GetComponent<Rigidbody>().AddForce((target.transform.position - transform.position).normalized * leftClawForwardForce, ForceMode.Impulse);
            ai.enableRotation = false;
        }
    }

    public void EndAttack()
    {
        fsm.SetCurrentState("Follow");
    }
}
