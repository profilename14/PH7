using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class VitriclawJumpSlam : AttackState
{
    [SerializeField]
    private ClipTransition jumpAnimation;

    private EnemyMovementController movementController;
    private SlashAttackVFXManager vfx;
    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    [SerializeField]
    private float windupRotationSpeed;

    [SerializeField]
    private float jumpDuration;

    private bool startJump;

    private float jumpTimer;

    private Vector3 endpoint;

    private bool updateEndpoint;

    private Vector3 startPoint;

    [SerializeField] GameObject puddle;

    [SerializeField] float groundY;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        vfx = (SlashAttackVFXManager)_Character.VFXManager;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        startJump = false;
        updateEndpoint = true;

        _ActionManager.SetAllActionPriorityAllowed(false);

        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(true);
        movementController.SetForceManualRotation(true);
        movementController.SetForceLookAtPlayer(true);

        AnimancerState currentState = _ActionManager.anim.Play(jumpAnimation);
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

        movementController.pathfinding.rotationSpeed = windupRotationSpeed;
        startPoint = character.transform.position;
        jumpTimer = 0;
        _Character.SetIsKnockbackImmune(true);
    }

    protected override void OnDisable()
    {
        movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
        movementController.pathfinding.rotationSpeed = character.characterData.rotationSpeed;
        movementController.SetAllowRotation(true);
    }

    private void Update()
    {
        if(updateEndpoint) endpoint = new Vector3(Player.instance.transform.position.x, groundY, Player.instance.transform.position.z);
        
        if (startJump)
        {
            jumpTimer += Time.deltaTime;
            float dashProgress = jumpTimer / jumpDuration;

            if (dashProgress > 1)
            {
                return;
            }

            Vector3 newPos = Vector3.Lerp(startPoint, endpoint, Mathf.Sqrt(dashProgress));

            movementController.gameObject.transform.position = newPos;
        }
    }

    public void StopTracking()
    {
        updateEndpoint = false;
    }

    public void JumpStart()
    {
        _Character.SetIsKnockbackImmune(true);
        startJump = true;
    }

    public void JumpEnd()
    {
        _Character.SetIsKnockbackImmune(false);
        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(false);
        Instantiate(puddle, new Vector3(transform.position.x, groundY, transform.position.z), Quaternion.identity);
    }
}
