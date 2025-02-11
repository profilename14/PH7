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

    // Uses allowedActions to control if entering this state is allowed.
    public override bool CanEnterState 
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Low];

    protected virtual void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
        player = (Player)_Character;
        vfx = (PlayerVFXManager)player.VFXManager;
        chargeAttackAnimation.Events.SetCallback(StartSwordSwingEvent, this.StartSwordSwing);
        chargeAttackAnimation.Events.SetCallback(EndSwordSwingEvent, this.EndSwordSwing);
    }

    protected override void OnEnable()
    {
        attackCharged = false;

        _ActionManager.SetAllActionPriorityAllowed(false);

        rotationController.snapToCurrentMouseAngle();

        _ActionManager.anim.Play(chargingAnimation);
    }

    protected override void OnDisable()
    {
        _ActionManager.SetAllActionPriorityAllowed(true);
    }

    public void ReleaseChargeAttack()
    {
        if (attackCharged)
        {
            _ActionManager.SetAllActionPriorityAllowed(false);

            AnimancerState currentState = _ActionManager.anim.Play(chargeAttackAnimation);

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

    public override void OnAttackHit(Vector3 position)
    {
        vfx.SwordHitVFX(position);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
