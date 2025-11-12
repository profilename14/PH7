using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class PlayerChargeAttack : AttackState
{
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private RotationController rotationController;

    [SerializeReference]
    ClipTransition chargingAnimation;

    [SerializeReference]
    ClipTransition chargeAttackAnimation;

    private static readonly StringReference StartSwordSwingEvent = "StartSwordSwing";
    private static readonly StringReference EndSwordSwingEvent = "EndSwordSwing";

    [SerializeField]
    AudioClip chargingSFX;

    [SerializeField]
    AudioClip chargeSwingSFX;

    [SerializeField]
    bool attackCharged;

    private Player player;
    private PlayerVFXManager vfx;
    private PlayerActionManager actionManager;

    private PlayerDirectionalInput directionalInput = new PlayerDirectionalInput();

    [SerializeField]
    UnityEvent onChargeRelease;

    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    float alkalineCost;

    // Uses allowedActions to control if entering this state is allowed.
    public override bool CanEnterState 
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Low] && playerStats.alkaline >= alkalineCost;

    protected virtual void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
        player = (Player)_Character;
        vfx = (PlayerVFXManager)player.VFXManager;
        chargeAttackAnimation.Events.SetCallback(StartSwordSwingEvent, this.StartSwordSwing);
        chargeAttackAnimation.Events.SetCallback(EndSwordSwingEvent, this.EndSwordSwing);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        attackCharged = false;

        directionalInput = actionManager.GetDirectionalInput();

        _ActionManager.SetAllActionPriorityAllowed(false);

        rotationController.snapToCurrentMouseAngle();

        movementController.SetAllowMovement(false);

        _ActionManager.anim.Play(chargingAnimation);
    }

    protected override void OnDisable()
    {
        movementController.SetAllowMovement(true);
        _ActionManager.SetAllActionPriorityAllowed(true);
    }

    public void ReleaseChargeAttack()
    {
        if (attackCharged)
        {
            _ActionManager.SetAllActionPriorityAllowed(false);

            AnimancerState currentState = _ActionManager.anim.Play(chargeAttackAnimation);

            playerStats.ModifyAlkaline(-alkalineCost);

            onChargeRelease.Invoke();

            // Do a charge attack, go back to idle at the end.
            currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
        else
        {
            _ActionManager.StateMachine.ForceSetDefaultState();
        }
    }

    public void ChargingDone()
    {
        attackCharged = true;
        vfx.FullyChargedVFX();
    }

    void StartSwordSwing()
    {
        vfx.SwordSwingVFX(SwordSwingType.ChargedSwing);
    }

    void EndSwordSwing()
    {
        vfx.EndChargeVFX();
        movementController.SetAllowRotation(true);
    }

    public Vector3 GetAttackingDirection()
    {
        return movementController.GetCameraPlanarRotation() * directionalInput.lookDir;
    }

    public override void OnAttackHit(Vector3 position, Collider other)
    {
        vfx.SwordHitVFX(position);

        if (movementController.cinemachineManager)
        {
            movementController.cinemachineManager.ScreenShake();
        }

        vfx.PauseSwordSwingVFX(SwordSwingType.ChargedSwing, 0.25f);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
