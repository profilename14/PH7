using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Patterns;

public class StriderAI : EnemyAI
{
    [Header("State Transition Variables")]
    public float timeToRedecideState;

    [Header("Next State Info")]
    public string nextChosenState;
    private float nextChosenAttackRange;
    public float timeToAttackNext;
    private float attackTimer;
    private float redecideStateTimer;

    [Header("Charge Attack")]
    public float chargeAttackRange;
    public float chargeAdditionalDelay;
    public float chargeForwardForce;
    public float dashForce;

    [Header("Randomized Delay Range")]
    public float minTimeToAttack = 0.5f; // Min time after reaching chosen attack range to begin attack
    public float maxTimeToAttack = 2f; // Max time after reaching chosen attack range to begin attack

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
       
        fsm.Add("Charge", new EnemyState(fsm, "Charge", this));
        Init_Follow();
        Init_Charge();
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
            redecideStateTimer = 0;
            nextChosenState = "Charge";
            nextChosenAttackRange = chargeAttackRange;
            timeToAttackNext = Random.Range(minTimeToAttack, maxTimeToAttack) + chargeAdditionalDelay;
        };

        state.OnUpdateDelegate += delegate ()
        {
            //if(Vector3.Distance(this.transform.position, player.transform.position) < nextChosenAttackRange)
            attackTimer += Time.deltaTime;

            if(attackTimer > timeToAttackNext)
            {
                fsm.SetCurrentState(nextChosenState);
            }
        };
    }

    void Init_Charge()
    {
        EnemyState state = (EnemyState)fsm.GetState("Charge");

        state.OnEnterDelegate += delegate ()
        {
            ai.isStopped = true;
            ai.enableRotation = false;
            target.transform.position = player.transform.position;
            anim.SetTrigger("Charge");
        };

        state.OnExitDelegate += delegate ()
        {
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
        if(state == "Charge")
        {
            ai.enableRotation = false;
            ai.isStopped = true;
            DashImpulse();
        }
    }

    public IEnumerator Dash()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        while(true)
        {
            target.transform.position = player.transform.position;
            rb.AddForce((target.transform.position - transform.position).normalized * dashForce, ForceMode.Impulse);
            yield return null;
        }
    }

    public void DashImpulse()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        target.transform.position = player.transform.position;
        rb.AddForce((target.transform.position - transform.position).normalized * dashForce, ForceMode.Impulse);
    }

    public void DashStop()
    {
        StopAllCoroutines();
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<CapsuleCollider>().enabled = true;
        ai.isStopped = true;
        ai.enableRotation = false;
        GetComponent<Rigidbody>().AddForce(transform.forward * chargeForwardForce, ForceMode.Impulse);
    }

    public void EndAttack()
    {
        GetComponent<CapsuleCollider>().enabled = true;
        fsm.SetCurrentState("Follow");
    }
}
