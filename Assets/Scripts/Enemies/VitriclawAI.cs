using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Patterns;

public class VitriclawAI : EnemyAI
{
    [Header("State Transition Variables")]
    public float followToLeftClawProbability;
    public float followToRightClawProbability;
    public float followToJumpProbability;
    public float timeToRedecideState;

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
    //public bool leftClawAfterHitstun;

    [Header("Right Claw Attack")]
    public float rightClawAttackRange;
    public float rightClawAdditionalDelay;
    public float rightClawForwardForce;
    public float dashForce;
    public bool isDashing;

    [Header("Jump Attack")]
    public float jumpAttackRange;
    public float jumpAdditionalDelay;
    public float jumpInitialForce;
    public float jumpPeakForce;
    public float jumpLandingDelay;
    public AudioClip jumpLandSound;

    [Header("Randomized Delay Range")]
    public float minTimeToAttack = 0.5f; // Min time after reaching chosen attack range to begin attack
    public float maxTimeToAttack = 2f; // Max time after reaching chosen attack range to begin attack

    private bool clawRaiseFlag = false;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        fsm.Add("Left Claw", new EnemyState(fsm, "Left Claw", this));
        fsm.Add("Right Claw", new EnemyState(fsm, "Right Claw", this));
        fsm.Add("Jump", new EnemyState(fsm, "Jump", this));
        Init_Follow();
        Init_Hitstun();
        Init_Left_Claw();
        Init_Jump();
        Init_Right_Claw();
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

            float num = Random.value;
            if(num <= followToLeftClawProbability)
            {
                nextChosenState = "Left Claw";
                nextChosenAttackRange = leftClawAttackRange;
                timeToAttackNext = Random.Range(minTimeToAttack, maxTimeToAttack) + leftClawAdditionalDelay;
            }
            else if(num <= followToLeftClawProbability + followToJumpProbability)
            {
                nextChosenState = "Jump";
                nextChosenAttackRange = jumpAttackRange;
                timeToAttackNext = Random.Range(minTimeToAttack, maxTimeToAttack) + jumpAdditionalDelay;
            }
            else
            {
                nextChosenState = "Right Claw";
                nextChosenAttackRange = rightClawAttackRange;
                timeToAttackNext = Random.Range(minTimeToAttack, maxTimeToAttack) + rightClawAdditionalDelay;
            }
        };

        state.OnUpdateDelegate += delegate ()
        {
            if(Vector3.Distance(this.transform.position, player.transform.position) < nextChosenAttackRange)
            {
                attackTimer += Time.deltaTime;

                if(attackTimer > timeToAttackNext)
                {
                    fsm.SetCurrentState(nextChosenState);
                }
            }
            else
            {
                redecideStateTimer += Time.deltaTime;

                if(redecideStateTimer > timeToRedecideState)
                {
                    if(Vector3.Distance(this.transform.position, player.transform.position) < rightClawAttackRange && Random.value > 0.4f)
                    {
                        fsm.SetCurrentState("Right Claw");
                    }
                    else
                    {
                        fsm.SetCurrentState("Jump");
                    }
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

            if(isCircling)
            {
                if (!clawRaiseFlag)
                {
                    clawRaiseFlag = true;
                    if (Random.value >= 0.5)
                    {
                        anim.SetInteger("Walk Dir [-1, 0, 1]", 1);
                        isCirclingRight = true;
                        anim.SetBool("Right Claw Raised", true);
                    }
                    else
                    {
                        anim.SetInteger("Walk Dir [-1, 0, 1]", -1);
                        isCirclingRight = false;
                        anim.SetBool("Right Claw Raised", false);
                    }
                }
            }
            else
            {
                clawRaiseFlag = false;
                anim.SetInteger("Walk Dir [-1, 0, 1]", 0);
            }
        };
    }

    void Init_Hitstun()
    {
        EnemyState state = (EnemyState)fsm.GetState("Hitstun");

        state.OnExitDelegate += delegate ()
        {
            //fsm.SetCurrentState("Left Claw");
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

        state.OnExitDelegate += delegate ()
        {
            ai.isStopped = false;
            ai.enableRotation = true;
        };
    }

    void Init_Jump()
    {
        EnemyState state = (EnemyState)fsm.GetState("Jump");

        state.OnEnterDelegate += delegate ()
        {
            ai.rotationSpeed = 720;
            ai.isStopped = true;
            ai.enableRotation = true;
            target.transform.position = player.transform.position;
            anim.SetTrigger("Jump");
        };

        state.OnExitDelegate += delegate ()
        {
            ai.rotationSpeed = defaultRotationSpeed;
            ai.isStopped = false;
            ai.enableRotation = true;
        };
    }

    void Init_Right_Claw()
    {
        EnemyState state = (EnemyState)fsm.GetState("Right Claw");

        state.OnEnterDelegate += delegate ()
        {
            ai.rotationSpeed = 720;
            ai.isStopped = true;
            ai.enableRotation = true;
            target.transform.position = player.transform.position;
            anim.SetTrigger("Right Claw");
        };

        state.OnExitDelegate += delegate ()
        {
            ai.rotationSpeed = defaultRotationSpeed;
            ai.isStopped = false;
            ai.enableRotation = true;
        };
    }

    public void StartAttack(string state)
    {
        if(state == "Left Claw")
        {
            GetComponent<Rigidbody>().AddForce(transform.forward * leftClawForwardForce, ForceMode.Impulse);
            ai.enableRotation = false;
        }
        else if(state == "Jump")
        {
            inPuddle = false;
            target.transform.position = player.transform.position;
            ai.isStopped = false; 
            GetComponent<Rigidbody>().AddForce(transform.forward * jumpInitialForce, ForceMode.Impulse);
            GetComponent<CapsuleCollider>().enabled = false;
            //ai.maxSpeed = 50;
            //ai.acceleration = 10000;
        }
        else if(state == "Right Claw")
        {
            ai.enableRotation = true;
            ai.isStopped = false;
            isDashing = true;
            StartCoroutine(Dash());
        }
    }

    public IEnumerator Dash()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        while(!isHitstunned && isDashing)
        {
            target.transform.position = player.transform.position;
            rb.AddForce((target.transform.position - transform.position).normalized * dashForce, ForceMode.Impulse);
            yield return null;
        }
    }

    public void DashStop()
    {
        isDashing = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<CapsuleCollider>().enabled = true;
        ai.isStopped = true;
        ai.enableRotation = false;
        GetComponent<Rigidbody>().AddForce(transform.forward * rightClawForwardForce, ForceMode.Impulse);
    }

    public void JumpPeak()
    {
        target.transform.position = player.transform.position;
        GetComponent<Rigidbody>().AddForce((target.transform.position - transform.position).normalized * jumpPeakForce, ForceMode.Impulse);
    }

    public void LandJump()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<CapsuleCollider>().enabled = true;
        ai.isStopped = true;
        ai.enableRotation = false;
        audioSource.PlayOneShot(jumpLandSound, 0.375F);
        PauseStartupForSeconds(jumpLandingDelay);
    }

    public void EndAttack()
    {
        inInterruptFrames = true;
        GetComponent<CapsuleCollider>().enabled = true;
        fsm.SetCurrentState("Follow");
    }
}
