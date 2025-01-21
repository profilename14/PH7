using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using PixelCrushers.DialogueSystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine.InputSystem.LowLevel;

public enum PlayerRotationState
{
    Default,
    Locked
}

public enum BonusOrientationMethod
{
    None,
    TowardsGravity,
    TowardsGroundSlopeAndGravity,
}

public class PlayerMovementController : MonoBehaviour, ICharacterController, ICharacterMovementController
{
    public KinematicCharacterMotor Motor;

    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15f;
    public float OrientationSharpness = 10f;

    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 15f;
    public float AirAccelerationSpeed = 15f;
    public float Drag = 0.1f;

    [Header("Misc")]
    public List<Collider> IgnoredColliders = new List<Collider>();
    public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
    public float BonusOrientationSharpness = 10f;
    public Vector3 gravity;
    public Vector3 defaultGravity = new Vector3(0, -30f, 0);
    public Transform MeshRoot;
    public Transform CameraFollowPoint;

    public PlayerRotationState CurrentRotationState { get; private set; }

    private Collider[] _probedColliders = new Collider[8];
    private RaycastHit[] _probedHits = new RaycastHit[8];

    private Vector3 moveInputVector;
    private Vector3 _lookInputVector;

    private bool AllowJumpingWhenSliding = false;
    private float jumpUpSpeed = 10f;
    private float jumpPreGroundingGraceTime = 0f;
    private float jumpPostGroundingGraceTime = 0f;
    private bool jumpHeld = false;
    private bool jumpPressedThisFrame = false;
    private bool inJump = false;
    private bool jumpedThisFrame = false;
    private float timeSinceJumpRequested = Mathf.Infinity;
    private float timeSinceLastAbleToJump = 0f;

    private Vector3 _internalVelocityAdd = Vector3.zero;

    private Vector3 lastInnerNormal = Vector3.zero;
    private Vector3 lastOuterNormal = Vector3.zero;

    private GameObject CharacterCamera;

    [SerializeField]
    private GameObject rotationRoot;

    // Custom Variables Below:
    public bool isDashing = false;
    
    bool canMove = true;

    bool setVelocity = false;
    private Vector3 _internalVelocitySet = Vector3.zero;
    
    bool setPosition = false;
    private Vector3 _internalPositionSet = Vector3.zero;

    bool velocityLocked = false;
    private Vector3 lockedVelocity;
    private Quaternion savedLockedRotation;
    private Quaternion savedUpdatedRotation;
    private Vector3 camForward;
    private Vector3 camRight;

    private void Awake()
    {
        // Handle initial state
        TransitionToState(PlayerRotationState.Default);

        // Assign the characterController to the motor
        Motor.CharacterController = this;

        CharacterCamera = GameObject.FindGameObjectWithTag("MainCamera");

        camForward = CharacterCamera.transform.forward;
        camRight = CharacterCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        gravity = defaultGravity;
    }

    private void Update()
    {

    }

    /// <summary>
    /// Handles movement state transitions and enter/exit callbacks
    /// </summary>
    public void TransitionToState(PlayerRotationState newState)
    {
        PlayerRotationState tmpInitialState = CurrentRotationState;
        OnStateExit(tmpInitialState, newState);
        CurrentRotationState = newState;
        OnStateEnter(newState, tmpInitialState);
    }

    /// <summary>
    /// Event when entering a state
    /// </summary>
    public void OnStateEnter(PlayerRotationState state, PlayerRotationState fromState)
    {
        switch (state)
        {
            case PlayerRotationState.Default:
                {
                    break;
                }
            case PlayerRotationState.Locked:
                {
                    savedLockedRotation = transform.rotation;
                    break;
                }
        }
    }

    /// <summary>
    /// Event when exiting a state
    /// </summary>
    public void OnStateExit(PlayerRotationState state, PlayerRotationState toState)
    {
        switch (state)
        {
            case PlayerRotationState.Default:
                {
                    break;
                }
            case PlayerRotationState.Locked:
                {
                    break;
                }
        }
    }

    /// <summary>
    /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
    /// </summary>
    public void ProcessMoveInput(Vector3 moveDir)
    {
        if (!canMove)
        {
            moveInputVector = new Vector3(0, 0, 0);
            return;
        }

        Quaternion cameraRotation = CharacterCamera.transform.rotation;

        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(cameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(cameraRotation * Vector3.up, Motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        // Move and look inputs
        moveInputVector = cameraPlanarRotation * moveDir;


        // Jumping input
        if (jumpHeld)
        {
            timeSinceJumpRequested = 0f;
        }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime)
    {
        // Only place you can modify position!
        if (setPosition == true)
        {
            transform.position = _internalPositionSet;
        }
        setPosition = false;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its rotation should be right now. 
    /// This is the ONLY place where you should set the character's rotation
    /// </summary>
    public void UpdateRotation(ref Quaternion rotation, float deltaTime)
    {
        // This function is disabled for now as reotation controller already works. However, if we ever move past said script,
        // there's a partial implementation here for rotation being done along movement.

        
        switch (CurrentRotationState)
        {
            case PlayerRotationState.Locked:
                {
                    transform.rotation = savedLockedRotation;
                    break;
                }
            case PlayerRotationState.Default:
                {
                    if (OrientationSharpness > 0f)
                    {
                        // Smoothly interpolate from current to target look direction
                        Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, savedUpdatedRotation * Vector3.forward, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
                        
                        // Set the current rotation (which will be used by the KinematicCharacterMotor)
                        transform.rotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                    }

                    

                    break;
                }
        }

        rotation = transform.rotation;
        
    }



    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if(velocityLocked)
        {
            currentVelocity = lockedVelocity;
            return;
        }

        // Moved this here so it can be differentiated from LockVelocity
        if (setVelocity)
        {
            setVelocity = false;
            currentVelocity = _internalVelocitySet;
            _internalVelocitySet = Vector3.zero;
        }

        // Ground movement
        if (Motor.GroundingStatus.IsStableOnGround || isDashing) // Ensures friction applies during dashes, making them brief midair
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;

            Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

            // Reorient velocity on slope
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(moveInputVector, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInputVector.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
        }
        // Air movement
        else
        {
            // Add move input
            if (moveInputVector.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = moveInputVector * AirAccelerationSpeed * deltaTime;

                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
                {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                    Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                    {
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                    }
                }

                // Prevent air-climbing sloped walls
                if (Motor.GroundingStatus.FoundAnyGround)
                {
                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                    }
                }

                // Apply added velocity
                currentVelocity += addedVelocity;
            }
            else
            {
                currentVelocity -= new Vector3(currentVelocity.x, 0, currentVelocity.z);
            }

            // Gravity
            currentVelocity += gravity * deltaTime;

            // Drag
            currentVelocity *= (1f / (1f + (Drag * deltaTime)));

        }

        // Handle jumping
        jumpedThisFrame = false;
        timeSinceJumpRequested += deltaTime;

        // Should only start a new jump if the button is pressed while on jumpable ground
        // If the player is already in a jump for < maxJumpTime, then can continue gaining height
        if ((jumpPressedThisFrame && IsAbleToJump()) || (jumpHeld && inJump))
        {
            inJump = true;

            // Makes the character skip ground probing/snapping on its next update. 
            // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
            Motor.ForceUnground();

            // Add to the return velocity and reset jump state
            currentVelocity = new Vector3(currentVelocity.x, jumpUpSpeed, currentVelocity.z);
            jumpedThisFrame = true;
        }
        else
        {
            inJump = false;
        }

        jumpPressedThisFrame = false;

        // Take into account additive velocity
        if (_internalVelocityAdd.sqrMagnitude > 0f)
        {
            currentVelocity += _internalVelocityAdd;
            _internalVelocityAdd = Vector3.zero;
        }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime)
    {

        // Handle jumping pre-ground grace period
        if (jumpHeld && timeSinceJumpRequested > jumpPreGroundingGraceTime)
        {
            jumpHeld = false;
        }

        if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
        {
            // If we're on a ground surface, reset jumping values
            if (!jumpedThisFrame)
            {
                inJump = false;
            }
            timeSinceLastAbleToJump = 0f;
        }
        else
        {
            // Keep track of time since we were last able to jump (for grace period)
            timeSinceLastAbleToJump += deltaTime;
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        // Handle landing and leaving ground
        if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLanded();
        }
        else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLeaveStableGround();
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        if (isDashing && coll.CompareTag("PhaseableWallController")) {
            return false;
        }

        if (IgnoredColliders.Contains(coll))
        {
            return false;
        }

        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void AddVelocity(Vector3 velocity)
    {
        _internalVelocityAdd += velocity;
    }

    public void SetVelocity(Vector3 velocity)
    {
        setVelocity = true;
        _internalVelocitySet = velocity;
    }

    public void SetPosition(Vector3 position)
    {
        setPosition = true;
        _internalPositionSet = position;
    }

    public void LockVelocity(Vector3 velocity)
    {
        velocityLocked = true;
        lockedVelocity = velocity;
    }

    public void UnlockVelocity()
    {
        velocityLocked = false;
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    protected void OnLanded()
    {
    }

    protected void OnLeaveStableGround()
    {
    }

    public void OnDiscreteCollisionDetected(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            GameObject enemy = other.gameObject;


            // Note: phasing can be handled in this script, but should be controlled via method calls from the Dash State

            if (isDashing) { // Phase through gates, but only if dashing.
            //Debug.Log("DISABLINGCOLLISION");
            Physics.IgnoreCollision(
                other,
                GetComponent<Collider>(), true);
                //StartCoroutine(ReenableCollision(dashDuration - DashTimer, enemy.gameObject.GetComponent<Collider>()));
            } else { // Can't bypass
            //Debug.Log("ENABLINGCOLLISION");
                
            }
        }
    }

    private IEnumerator ReenableCollision(float waitTime, Collider collider)
    {
        yield return new WaitForSeconds(waitTime + 0.1f); // just to be safe
            
        Physics.IgnoreCollision(
                collider,
                GetComponent<Collider>(), false);
    }

    //
    // CUSTOM FUNCTIONS
    //

    public void PassJumpData(float jumpUpSpeed, float jumpPreGroundingGraceTime, float jumpPostGroundingGraceTime)
    {
        this.jumpUpSpeed = jumpUpSpeed;
        this.jumpPreGroundingGraceTime = jumpPreGroundingGraceTime;
        this.jumpPostGroundingGraceTime = jumpPostGroundingGraceTime;
    }

    public void SetJumpVelocity(float jumpUpSpeed)
    {
        this.jumpUpSpeed = jumpUpSpeed;
    }

    public void SetGravityScale(float gravityScale)
    {
        gravity = defaultGravity * gravityScale;
    }

    public void StartJump()
    {
        jumpHeld = true;
        jumpPressedThisFrame = true;
    }

    public void StopJump()
    {
        //Debug.Log("stopping jump");
        jumpHeld = false;
    }

    public void ApplyImpulseForce(Vector3 direction, float power)
    {
        AddVelocity(direction.normalized * power);
    }

    public void AddSpeedModifier(float modifier)
    {
        throw new NotImplementedException();
    }

    public void RemoveSpeedModifier(float modifier)
    {
        throw new NotImplementedException();
    }

    public bool IsGrounded()
    {
        return Motor.GroundingStatus.IsStableOnGround;
    }

    public bool IsAbleToJump()
    {
        return AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : IsGrounded() || timeSinceLastAbleToJump <= jumpPostGroundingGraceTime;
    }

    public void SetAllowMovement(bool isAllowed)
    {
        throw new NotImplementedException();
    }

    public void SetDrag(float drag)
    {
        throw new NotImplementedException();
    }

    public void SetAllowRotation(bool isAllowed)
    {
        if (!isAllowed) {
            TransitionToState(PlayerRotationState.Locked);
            return;
        }

        TransitionToState(PlayerRotationState.Default);
    }

    // Called by external scripts to set rotation.
    public void RotateToDir(Vector3 dir)
    {
        if (dir != Vector3.zero)
        {
            savedUpdatedRotation = Quaternion.LookRotation(dir, Motor.CharacterUp);
        }
    }
}