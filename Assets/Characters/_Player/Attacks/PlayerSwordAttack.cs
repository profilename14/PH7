using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public enum SwordSwingType { Swing0, Swing1, Swing2, SwingDown, ChargedSwing, DashSwing }

public class PlayerSwordAttack : AttackState
{
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private RotationController rotationController;

    
    public override string StateName => "PlayerSwordAttack";
    
    [SerializeField]
    CinemachineManager cinemachineManager;

    // Just start at maxValue so it will always have first swing at 0.
    int currentSwing = int.MaxValue;

    [SerializeField, EventNames]
    ClipTransition[] attackAnimations;

    [SerializeField, EventNames]
    ClipTransition downwardsSwingAnimation;

    private static readonly StringReference StartSwordSwingEvent = "StartSwordSwing";
    private static readonly StringReference EndSwordSwingEvent = "EndSwordSwing";

    [SerializeField]
    AudioClip swordSwingSFX;

    [SerializeField]
    float swingForce = 40f;
    
    [SerializeField]
    float hitStopTime = 0.15f;

    [SerializeField]
    float hitRecoilForce;

    [SerializeField]
    float pogoForce;

    [SerializeField]
    float startNewComboCooldown = 0.2f;

    [SerializeField]
    float drag;

    private bool isPogo;

    private AnimancerState currentState;

    private PlayerVFXManager vfx;

    private Player player;

    private PlayerActionManager actionManager;

    private PlayerStats playerStats;

    private SwordSwingType currentSwordSwing;

    private PlayerDirectionalInput directionalInput = new PlayerDirectionalInput();

    private Vector3 moveDirAtStart; 

    private float baseKnockback;
    private float baseFinisherKnockback;

    private bool canDashAttack = true;

    private bool isDone;

    public AudioSource audioSource;

    // Uses allowedActions to control if entering this state is allowed.
    // Also must have animations in the array.
    public override bool CanEnterState 
        => attackAnimations.Length > 0 && _ActionManager.allowedActionPriorities[CharacterActionPriority.Medium] && !(actionManager.pogoTimer > 0 && !movementController.IsGrounded());

    protected void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
        gameObject.GetComponentInParentOrChildren(ref playerStats);


        player = (Player)_Character;
        vfx = (PlayerVFXManager)player.VFXManager;

        for(int i = 0; i < attackAnimations.Length; i++)
        {
            attackAnimations[i].Events.SetCallback(StartSwordSwingEvent, this.StartSwordSwing);
            attackAnimations[i].Events.SetCallback(EndSwordSwingEvent, this.EndSwordSwing);
            attackAnimations[i].Events.SetCallback(AllowHighPriorityEvent, AllowHighPriority);
        }

        downwardsSwingAnimation.Events.SetCallback(StartSwordSwingEvent, StartSwordSwing);
        downwardsSwingAnimation.Events.SetCallback(EndSwordSwingEvent, EndSwordSwing);
        downwardsSwingAnimation.Events.SetCallback(AllowHighPriorityEvent, AllowHighPriority);

        baseKnockback = _AttackDataClone.knockback;
        baseFinisherKnockback = _AttackDataClone.knockback * 1.15f; // Todo: raise this when the cooldown on swing 3 is increased
    }


    protected override void OnEnable()
    {
        base.OnEnable();

        audioSource.PlayOneShot(swordSwingSFX);
        directionalInput = actionManager.GetDirectionalInput();

        moveDirAtStart = directionalInput.lookDir;

        // Fully committed to an attack once you start it.
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Medium, false);
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.High, true);

        canDashAttack = true;

        isDone = false;

        if (movementController.IsGrounded())
        {
            movementController.RotateToDir(moveDirAtStart);
            movementController.SetAllowRotation(false);

            movementController.SetGroundDrag(drag);
            movementController.SetVelocity(movementController.GetCameraPlanarRotation() * directionalInput.lookDir * swingForce);
            movementController.SetAllowMovement(false);

            isPogo = false;

            if (currentSwing >= attackAnimations.Length - 1 || currentState == null || currentState.Weight == 0)
            {
                currentSwing = 0;
            }
            else
            {
                currentSwing++;
            }

            //Debug.Log("Current swing: " + currentSwing);

            currentSwordSwing = (SwordSwingType)currentSwing;

            currentState = _ActionManager.anim.Play(attackAnimations[currentSwing]);

            if(currentSwing == 0 || currentSwing == 1) {
                _AttackDataClone.knockback = baseKnockback;
            }

            if(currentSwing == 2) {
                _ActionManager.SetAllActionPriorityAllowed(true, currentState.Duration);

                _AttackDataClone.knockback = baseFinisherKnockback;
            }



            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
        else
        {
            movementController.SetAllowRotation(true);

            actionManager.PogoCooldown();
            isPogo = true;
            

            currentSwordSwing = SwordSwingType.SwingDown;

            // Swinging in the air performs a downwards swing.
            currentState = _ActionManager.anim.Play(downwardsSwingAnimation);

            _ActionManager.SetAllActionPriorityAllowed(true, currentState.Duration);

            // Just sets to idle after this animation fully ends.
            currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;
        }
    }



    protected void Update()
    {
        if(currentSwordSwing == SwordSwingType.SwingDown)
        {
            if (!actionManager.lockedOn)
            {
                movementController.RotateToDir(Quaternion.Euler(0f, -45f, 0f) * directionalInput.moveDir);
            }
            else
            {
                movementController.RotateToDir(directionalInput.lookDir);
            }
        }

        if(isDone)
        {
            movementController.RotateToDir(directionalInput.moveDir);
        }

        /*if (actionManager.GetBufferedState() && (actionManager.GetBufferedState().StateName == "PlayerDash"
                                             || actionManager.GetBufferedState().StateName == "PlayerDashAttack"))
        {
            if (canDashAttack)
            {
                print("DashAttackFromAttackState");
                movementController.SetAllowRotation(true);
                actionManager.ForceDashAttackState();
            }

        }*/
    }

    public override void OnAttackHit(Vector3 position, Collider other)
    {
        vfx.SwordHitVFX(position);

        if (cinemachineManager)
        {
            cinemachineManager.ScreenShake();
        }
        
        vfx.PauseSwordSwingVFX(currentSwordSwing, hitStopTime);

        if(currentSwordSwing == SwordSwingType.SwingDown)
        {
            movementController.SetVelocity(Vector3.zero);

            float forceMod = 1;

            actionManager.RefreshDash();
            
            if (other.gameObject.GetComponentInParentOrChildren<Pogoable>())
            {

                forceMod = other.gameObject.GetComponentInParentOrChildren<Pogoable>().bouncinessMod;
            }
            else if (other.gameObject.GetComponentInParentOrChildren<Enemy>())
            {
                forceMod = other.gameObject.GetComponentInParentOrChildren<Enemy>().GetBounciness();
            }

            movementController.SetVelocity(movementController.gameObject.transform.transform.up * pogoForce * forceMod);
        }
        else
        {
            StartCoroutine(HitStop(hitStopTime));

            movementController.SetVelocity(movementController.gameObject.transform.transform.forward * hitRecoilForce);

            actionManager.hasDashedInAir = false;
        }
    }

    public void StartSwordSwing()
    {
        vfx.SwordSwingVFX(currentSwordSwing);

        canDashAttack = false;
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.High, false);

        
    }

    public void EndSwordSwing()
    {
        isDone = true;

        AllowMove();
        AllowJump();
        AllowLowPriority();
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);

        if(!isPogo && currentSwing >= attackAnimations.Length - 1)
        {
            //Debug.Log("resetting");
            currentSwing = -1;
            // Doing it like this instead of events, more consistency if exiting state
            actionManager.SetActionPriorityAllowed(CharacterActionPriority.Medium, startNewComboCooldown);
        }
        else
        {
            AllowMediumPriority();
        }
    }

    // Extra behavior right before/after hitting an enemy, calls AttackState's OnTriggerEnter.
    protected void OnTriggerEnter(Collider other)
    {
        float distanceKnockbackMultiplier = 1;
        float damageMultiplier = 1;

        // If an enemy is 3.5 units from Typhis, knockback is unchanged. If closer, its increased, if farther, its reduced. 
        // A base knockback of 15 can range from about 7.5 to 22.5 with this.
        distanceKnockbackMultiplier = ((transform.position - other.transform.position).magnitude - 3.5f) * 3f;
        damageMultiplier = 1 + 0.335f * GameManager.instance.damageUpgrade;


        if (other.gameObject.tag == "Enemy" || true)
        {
            //Debug.Log(distanceKnockbackMultiplier);

            _AttackDataClone.knockback = _AttackDataClone.knockback - distanceKnockbackMultiplier;
            _AttackDataClone.damage = _AttackDataClone.damage * (damageMultiplier);

        }

        //Debug.Log(_AttackDataClone.damage);
        
        //Debug.Log(damageMultiplier);

        // Call AttackState's OnTriggerEnter
        base.OnTriggerEnter(other);

        // Change knockback back
        if (other.gameObject.tag == "Enemy" || true)
        {
            _AttackDataClone.knockback = _AttackDataClone.knockback + distanceKnockbackMultiplier;
            _AttackDataClone.damage = _AttackDataClone.damage *= (1f/damageMultiplier);
        }
        

    }

    public Vector3 GetAttackingDirection()
    {
        if (currentSwordSwing == SwordSwingType.SwingDown)
        {
            return Vector3.down;
        }
        else
        {
            return movementController.GetCameraPlanarRotation() * directionalInput.lookDir;
        }
    }

    public Vector3 GetSaltShatterDirection()
    {
        return movementController.GetCameraPlanarRotation() * directionalInput.lookDir;
    }

    private IEnumerator HitStop(float duration)
    {
        currentState.IsPlaying = false;
        yield return new WaitForSeconds(duration);
        currentState.IsPlaying = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        movementController.ResetDrag();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
