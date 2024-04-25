using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Patterns;

public class VitriclawAI : EnemyAI
{
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
        //fsm.Add("Right Claw", new EnemyState(fsm, "Left Claw", this));
        fsm.Add("Jump", new EnemyState(fsm, "Left Claw", this));
        Init_Follow();
        Init_Left_Claw();
        Init_Jump();
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
            else if(num <= followToLeftClawProbability + followToJumpProbability)
            {
                nextChosenState = "Jump";
                nextStateIsAttack = true;
                nextChosenAttackRange = jumpAttackRange;
                timeToAttackNext = Random.Range(minTimeToAttack, maxTimeToAttack) + jumpAdditionalDelay;
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

            if (Vector3.Distance(this.transform.position, player.transform.position) <= followDistance)
            {
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

    void Init_Jump()
    {
        EnemyState state = (EnemyState)fsm.GetState("Jump");

        state.OnEnterDelegate += delegate ()
        {
            ai.rotationSpeed = 999;
            ai.isStopped = true;
            ai.enableRotation = true;
            target.transform.position = player.transform.position;
            anim.SetTrigger("Jump");
        };

        state.OnExitDelegate += delegate ()
        {
            ai.rotationSpeed = 360;
            //Debug.Log("OnExit - Left Claw");
            ai.isStopped = false;
            ai.enableRotation = true;
        };
    }

    public void PauseStartupForSeconds(float seconds)
    {
        StartCoroutine(PauseStartup(seconds));
    }

    public IEnumerator PauseStartup(float seconds)
    {
        anim.speed = 0;

        yield return new WaitForSeconds(seconds);

        anim.speed = 1;
    }

    public void StartAttack(string state)
    {
        if(state == "Left Claw")
        {
            GetComponent<Rigidbody>().AddForce((target.transform.position - transform.position).normalized * leftClawForwardForce, ForceMode.Impulse);
            ai.enableRotation = false;
        }
        else if(state == "Jump")
        {
            target.transform.position = player.transform.position;
            ai.isStopped = false;
            //ai.enableRotation = false;
            ai.maxSpeed = 50;
            ai.acceleration = 10000;
        }
    }

    public void EndAttack()
    {
        fsm.SetCurrentState("Follow");
    }
}
