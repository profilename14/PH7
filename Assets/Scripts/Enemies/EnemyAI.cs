using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor;

using Patterns;

public class EnemyAI : MonoBehaviour
{
    #region Base Enemy Data

    public FSM fsm;
    public RichAI ai;
    public Animator anim;

    protected GameObject player;

    [Header("Detection")]
    public bool playerDetected = false;
    public float sightDistance = 20f;

    [Header("Follow State")]
    public float followDistance = 1f;
    public float followMoveSpeed = 5;
    public float followAcceleration = 5;

    protected GameObject target;
    private LayerMask mask;

    public string currentState;

    #endregion

    // Start is called before the first frame update
    public void Start()
    {
        fsm = new FSM();
        player = GameObject.FindWithTag("Player");

        fsm.Add("Idle", new EnemyState(fsm, "Idle", this));
        fsm.Add("Follow", new EnemyState(fsm, "Follow", this));
        fsm.Add("Hitstun", new EnemyState(fsm, "Hitstun", this));
        fsm.Add("Die", new EnemyState(fsm, "Die", this));
        Init_Idle();
        Init_Follow();
        Init_Hitstun();
        Init_Die();

        fsm.SetCurrentState("Idle");

        mask = LayerMask.GetMask("Exclude from A*", "BlocksVision");

        target = new GameObject(this.gameObject.name + " AI Target");
        var iconContent = EditorGUIUtility.IconContent("sv_label_1");
        EditorGUIUtility.SetIconForObject(target, (Texture2D)iconContent.image);
    }

    // Update is called once per frame
    public void Update()
    {
        currentState = fsm.GetCurrentState();

        // Make sure the AI is always trying to move towards our target object
        ai.destination = target.transform.position;

        fsm.Update();
    }

    public void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    #region Initializing states

    void Init_Idle()
    {
        EnemyState state = (EnemyState)fsm.GetState("Idle");

        state.OnEnterDelegate += delegate ()
        {
            //Debug.Log("OnEnter - Idle");
            ai.isStopped = true;
        };

        state.OnExitDelegate += delegate ()
        {
            //Debug.Log("OnExit - Idle");
        };

        state.OnFixedUpdateDelegate += delegate ()
        {
            if(!playerDetected)
            {
                Physics.Raycast(transform.position + Vector3.up, (player.transform.position - transform.position).normalized, out RaycastHit hit, sightDistance, mask);

                Debug.DrawRay(transform.position + Vector3.up, (player.transform.position - transform.position).normalized * sightDistance);

                if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
                {
                    playerDetected = true;
                    fsm.SetCurrentState("Follow");
                }
            }
        };
    }

    void Init_Follow()
    {
        EnemyState state = (EnemyState)fsm.GetState("Follow");

        state.OnEnterDelegate += delegate ()
        {
            // Make sure that the AI is not stopped
            ai.isStopped = false;
            ai.enableRotation = true;
            ai.maxSpeed = followMoveSpeed;
            ai.acceleration = followAcceleration;

            anim.SetBool("Walking", true);
            ai.endReachedDistance = followDistance;
            //Debug.Log("OnEnter - Follow");
        };

        state.OnExitDelegate += delegate ()
        {
            //Debug.Log("OnExit - Follow");
            ai.endReachedDistance = 0.01f;
        };

        state.OnUpdateDelegate += delegate ()
        {
            //Debug.Log("OnUpdate - Follow");
            target.transform.position = player.transform.position;

            if(ai.reachedEndOfPath)
            {
                ai.isStopped = true;
                var direction = (target.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
            }
            else
            {
                ai.isStopped = false;
            }
        };
    }

    void Init_Hitstun()
    {
        EnemyState state = (EnemyState)fsm.GetState("Hitstun");

        state.OnEnterDelegate += delegate ()
        {
            // Make sure that the AI is stopped
            ai.isStopped = true;

            //Debug.Log("OnEnter - Follow");
        };

        state.OnExitDelegate += delegate ()
        {
            //Debug.Log("OnExit - Follow");
        };

        state.OnUpdateDelegate += delegate ()
        {
            //Debug.Log("OnUpdate - Follow");

            target.transform.position = player.transform.position;
        };
    }

    void Init_Die()
    {

    }
    #endregion
}