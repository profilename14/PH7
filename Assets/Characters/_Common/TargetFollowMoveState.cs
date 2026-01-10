using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class TargetFollowMoveState : CharacterState
{
    [SerializeField]
    private ClipTransition followAnimation;

    private EnemyMovementController movementController;
    public override bool CanEnterState => _ActionManager.allowedStates[this] && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium];

    private Vector3 startPoint;
    private float playerDistanceOffset;

    [SerializeField] float maxFollowTime;
    [SerializeField] float followDistance;
    [SerializeField] float followSpeed;
    [SerializeField] float rotationSpeed;

    [SerializeField] bool flipStrafeDirection;

    // Offset degrees from the forward vector that the enemy
    // will orient towards when moving towards target with
    // rotateTowardsTarget enabled.
    [SerializeField] float followForwardDirectionOffset;

    [SerializeField] float targetMoveDistanceMultiplier;

    [SerializeField] AttackCombo combo;
    [SerializeField] float enableComboAtPercent;
    
    private float moveTimer;

    private Vector3 startDirection;
    private Vector3 startForward;
    private float startAngle;

    private void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _ActionManager.SetAllActionPriorityAllowed(false);

        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(false);
        movementController.SetForceManualRotation(false);

        AnimancerState currentState = _ActionManager.anim.Play(followAnimation);
        //currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

        startPoint = character.transform.position;

        Vector3 pos1 = Player.instance.transform.position;
        pos1.y = 0;
        Vector3 pos2 = character.transform.position;
        pos2.y = 0;

        moveTimer = 0;

        playerDistanceOffset = Vector3.Distance(pos1, pos2);
        movementController.pathfinding.maxSpeed = followSpeed;
        startDirection = flipStrafeDirection ? character.transform.right * -1 : character.transform.right;
        startForward = character.transform.forward;
        startAngle = character.transform.rotation.y;
    }

    protected override void OnDisable()
    {
        movementController.pathfinding.maxSpeed = character.characterData.maxBaseMoveSpeed;
        movementController.pathfinding.rotationSpeed = character.characterData.rotationSpeed;
        movementController.SetAllowRotation(true);
        combo.SetAllowFollowup(false);
    }

    private void Update()
    {
        moveTimer += Time.deltaTime;
        float progress = moveTimer / maxFollowTime;

        if(progress > enableComboAtPercent) combo.SetAllowFollowup(true);


        if (progress > 1)
        {
            _ActionManager.StateMachine.ForceSetDefaultState();
        }

        //Debug.Log("Strafing progress " + progress);

        Vector3 nextTargetPos = startPoint + progress * targetMoveDistanceMultiplier * startDirection; //new Vector3(targetMoveXCurve.Evaluate(progress) * targetMoveDistanceMultiplier, 0, targetMoveYCurve.Evaluate(progress) * targetMoveDistanceMultiplier);

        //nextTargetPos = Quaternion.Euler(new Vector3(0, startAngle + targetMoveCurveAngleOffset, 0)) * nextTargetPos;

        //Debug.Log(targetMoveXCurve.Evaluate(progress) + " " + targetMoveYCurve.Evaluate(progress));

        movementController.SetPathfindingDestination(nextTargetPos);

        Vector3 dirToPlayer = (Player.instance.transform.position - character.transform.position).normalized;

        if (Vector3.Angle(startForward, dirToPlayer) < Vector3.Angle(-startForward, dirToPlayer)) character.gameObject.transform.rotation = movementController.pathfinding.SimulateRotationTowards(startForward, rotationSpeed * Time.deltaTime);
        else character.gameObject.transform.rotation = movementController.pathfinding.SimulateRotationTowards(-startForward, rotationSpeed * Time.deltaTime);
    }
}
