using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Pathfinding;

public class StriderJump : CharacterState
{
    [SerializeField]
    private ClipTransition jumpAnimation;

    [SerializeField]
    private float jumpDistance;

    [SerializeField]
    private float jumpDuration;

    private bool startJump;

    private float jumpTimer;

    private Vector3 endpoint;

    private EnemyMovementController movementController;
    private StriderVFXManager vfx;
    private Seeker seeker;

    private Vector3 startPoint;

    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        vfx = (StriderVFXManager)_Character.VFXManager;
        gameObject.GetComponentInParentOrChildren(ref seeker);
    }

    protected override void OnEnable()
    {
        Debug.Log("Enter jump state");

        startJump = false;

        _ActionManager.SetAllActionPriorityAllowed(false);
        _Character.SetIsKnockbackImmune(true);

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(false);
        movementController.SetForceManualRotation(false);

        RandomPath path = RandomPath.Construct(Player.instance.transform.position, (int) (1000 * jumpDistance));
        path.spread = 5000;

        seeker.StartPath(path, CalculateJump);

        //vfx.SetDashTrailEmission(true);

        startPoint = transform.position;

        jumpTimer = 0;

        AnimancerState currentState = _ActionManager.anim.Play(jumpAnimation);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
    }

    public void CalculateJump(Path p)
    {
        RandomPath path = (RandomPath)p;

        endpoint = path.endPoint;
    }

    public void StartJump()
    {
        startJump = true;
    }

    protected void Update()
    {
        if (startJump)
        {
            jumpTimer += Time.deltaTime;
            float dashProgress = jumpTimer / jumpDuration;

            if (dashProgress > 1)
            {
                return;
            }
            
            Vector3 newPos = Vector3.Lerp(startPoint, endpoint, Mathf.Sqrt(dashProgress));

            //Debug.Log("startPoint: " + startPoint);

            //Debug.Log("startPoint: " + endpoint);

            movementController.gameObject.transform.position = newPos;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
