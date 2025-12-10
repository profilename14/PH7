using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;


public class PlayerDashAttack : AttackState
{
    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private RotationController rotationController;

    
    public override string StateName => "PlayerDashAttack";
    


    [SerializeReference]
    ClipTransition attackAnimation;

    private static readonly StringReference StartSwordSwingEvent = "StartSwordSwing";
    private static readonly StringReference EndSwordSwingEvent = "EndSwordSwing";

    [SerializeField]
    AudioClip swordSwingSFX;

    [SerializeField]
    float hitStopTime = 0.15f;

    private AnimancerState currentState;

    private PlayerVFXManager vfx;

    private Player player;

    private PlayerActionManager actionManager;

    private PlayerDirectionalInput directionalInput = new PlayerDirectionalInput();

    private Vector3 moveDirAtStart; 

    private float baseKnockback;



    // Dash related variables

    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float dashTimer;
    [SerializeField] public AnimationCurve movementCurve;

    [SerializeField] float distance = 15;
    [SerializeField] float duration = 0.3f;
    private Vector3 startingPoint;
    private Vector3 destination;
    private Vector3 lookDir;
    public bool isDashing = false;
    [SerializeField] GameObject dashVFX;
    private GameObject instantiatedVFX;


    // Uses allowedActions to control if entering this state is allowed.
    // Also must have animations in the array.
    public override bool CanEnterState 
        => _ActionManager.allowedActionPriorities[CharacterActionPriority.Low] && actionManager.dashTimer <= -0.4f;

    protected void Awake()
    {
        base.Awake();
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref rotationController);
        gameObject.GetComponentInParentOrChildren(ref actionManager);

        player = (Player)_Character;
        vfx = (PlayerVFXManager)player.VFXManager;

        attackAnimation.Events.SetCallback(StartSwordSwingEvent, StartSwordSwing);
        attackAnimation.Events.SetCallback(EndSwordSwingEvent, EndSwordSwing);
        attackAnimation.Events.SetCallback(AllowHighPriorityEvent, AllowHighPriority);

        baseKnockback = _AttackDataClone.knockback;
    }


    protected override void OnEnable()
    {
        directionalInput = actionManager.GetDirectionalInput();

        moveDirAtStart = directionalInput.lookDir;

        // Fully committed to an attack once you start it.
        _ActionManager.SetActionPriorityAllowed(CharacterActionPriority.Medium, false);
        _ActionManager.SetAllActionPriorityAllowedExceptHitstun(false);

        movementController.RotateToDir(directionalInput.lookDir);

        

        movementController.SetAllowMovement(false);


        currentState = _ActionManager.anim.Play(attackAnimation);


        // Just sets to idle after this animation fully ends.
        currentState.Events(this).OnEnd ??= _ActionManager.StateMachine.ForceSetDefaultState;

        lookDir = directionalInput.lookDir;
        movementController.RotateToDir(lookDir);

        startingPoint = movementController.gameObject.transform.position;
        destination = new Vector3(startingPoint.x + lookDir.x * distance, 
                                  startingPoint.y,
                                  startingPoint.z + lookDir.z * distance);

        dashTimer = 0;
        isDashing = true;
        if (dashVFX && actionManager.dashTimer <= 0)
        {
            instantiatedVFX = Instantiate(dashVFX, transform);
            vfx.StartDashVFX();
        }


        //Debug.Log("AAAA");
        //Time.timeScale = 0.2f;
    }



    protected void Update()
    {
        dashTimer += Time.deltaTime;
        float dashProgress = dashTimer / duration;

        if (dashProgress > 1)
        {
            EndSwordSwing();
            return;
        }

        if (!Physics.Raycast(transform.position, movementController.transform.forward, 2))
        {
            Vector3 newPos = Vector3.Lerp( startingPoint, destination, movementCurve.Evaluate(dashProgress) );

            movementController.SetPosition(newPos);
            
        }
        else
        {
            EndSwordSwing();
        }
    }

    public override void OnAttackHit(Vector3 position, Collider other)
    {
        vfx.SwordHitVFX(position);

        if (Player.instance.cinemachineManager)
        {
            Player.instance.cinemachineManager.ScreenShake();
        }

        vfx.PauseSwordSwingVFX(SwordSwingType.DashSwing, hitStopTime);

        StartCoroutine(HitStop(hitStopTime));

        actionManager.hasDashedInAir = false; // chain air dash attacks if you hit enemies
    }

    public void StartSwordSwing()
    {
        vfx.SwordSwingVFX(SwordSwingType.DashSwing);
    }

    public void EndSwordSwing()
    {
        AllowMove();
        AllowJump();
        movementController.SetAllowMovement(true);
        movementController.SetAllowRotation(true);


        
        actionManager.EndDash(dashCooldown);
        vfx.EndDashVFX();
        isDashing = false;
        _ActionManager.SetAllActionPriorityAllowed(true, 0);
        //_ActionManager.StateMachine.ForceSetDefaultState();
        movementController.SetVelocity(new Vector3(0, 1f, 0)); // Effectively very slightly reduces gravity for a moment

        AllowMediumPriority();
    }

    // Extra behavior right before/after hitting an enemy, calls AttackState's OnTriggerEnter.
    protected void OnTriggerEnter(Collider other)
    {
        // Call AttackState's OnTriggerEnter
        base.OnTriggerEnter(other);
    }

    private IEnumerator HitStop(float duration)
    {
        currentState.IsPlaying = false;
        yield return new WaitForSeconds(duration);
        currentState.IsPlaying = true;
    }

    protected override void OnDisable()
    {
        if (instantiatedVFX)
        {
            Destroy(instantiatedVFX, 0.1f); // Give the vfx time to catch up to the player
        }
        
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
