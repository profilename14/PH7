using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

using Patterns;

public class EnemyAI : MonoBehaviour
{
    #region Base Enemy Data

    public FSM fsm;
    public RichAI richAI;

    private GameObject player;

    [Header("Detection")]
    public bool playerDetected = false;

    #endregion

    // Start is called before the first frame update
    public void Start()
    {
        fsm = new FSM();
        player = GameObject.FindWithTag("Player");

        fsm.Add("Idle", new EnemyState(fsm, "Idle", this));
        fsm.Add("FollowPlayer", new EnemyState(fsm, "FollowPlayer", this));
        fsm.Add("Flinch", new EnemyState(fsm, "Flinch", this));
        fsm.Add("Die", new EnemyState(fsm, "Die", this));
    }

    // Update is called once per frame
    void Update()
    {
        fsm.Update();
    }

    private void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    #region Initializing states

    void Init_Idle()
    {
        EnemyState state = (EnemyState)fsm.GetState("Idle");

        state.OnEnterDelegate += delegate ()
        {
            Debug.Log("OnEnter - Idle");
        };

        state.OnExitDelegate += delegate ()
        {
            Debug.Log("OnExit - Idle");
        };

        state.OnFixedUpdateDelegate += delegate ()
        {
            Debug.Log("OnFixedUpdate - Idle");


            if(!playerDetected)
            {
                Ray ray = new(transform.position, player.transform.position - transform.position);
                Physics.Raycast(ray, out RaycastHit hit, 50f, 1 << LayerMask.NameToLayer("BlocksVision"));

                if(hit.collider.gameObject.CompareTag("Player"))
                {
                    
                }
            }
        };
    }

    void Init_FollowPlayer()
    {

    }
    void Init_Flinch()
    {

    }
    void Init_Die()
    {

    }
    #endregion
}