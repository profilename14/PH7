using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using PixelCrushers.DialogueSystem;
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

public class PlayerMovementController : CharacterMovementController, ICharacterController
{
    public KinematicCharacterMotor Motor;

    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15f;
    public float OrientationSharpness = 10f;
    public float defaultGroundDrag = 1;
    private float groundDrag = 1;

    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 15f;
    public float AirAccelerationSpeed = 10f;
    public float defaultAirDrag = 0.1f;
    private float airDrag = 0.1f;

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

    [SerializeField]
    private Vector3 moveInputVector;
    private Vector3 lookInputVector;

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

    [SerializeField]
    public CinemachineManager cinemachineManager;

    // Custom Variables Below:
    public bool isDashing = false;

    public bool canMove = true;
    public bool canRotate = true;
    private bool ignoreSmoothRotation = false;

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

    private PlayerVFXManager playerVFXManager;

    [SerializeField]
    private bool isSprinting = false;
    [SerializeField]
    private float sprintSpeedMult = 2f;
    [SerializeField]
    private float sprintSharpnessMult = 0.3f; // Lower numbers make movement slippier and more acceleration based

    [SerializeField]
    private bool isSprintJump = false;


    private void Awake()
    {
        // Assign the characterController to the motor
        Motor.CharacterController = this;

        CharacterCamera = GameObject.FindGameObjectWithTag("MainCamera");

        playerVFXManager = gameObject.GetComponent<PlayerVFXManager>();

        camForward = CharacterCamera.transform.forward;
        camRight = CharacterCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        gravity = defaultGravity;
        airDrag = defaultAirDrag;
        groundDrag = defaultGroundDrag;
    }

    private void Update()
    {
        if (!canMove)
        {
            moveInputVector = new Vector3(0, 0, 0);
        }
    }

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

                        if (ignoreSmoothRotation)
                        {
                            smoothedLookInputDirection = savedUpdatedRotation * Vector3.forward;
                        }

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
        if (velocityLocked)
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

            Vector3 targetMovementVelocity;
            if (isSprinting)
            {
                targetMovementVelocity = reorientedInput * MaxStableMoveSpeed * sprintSpeedMult;
                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime * sprintSharpnessMult));
            }
            else
            {
                targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;
                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }



            // Drag
            currentVelocity *= (1f / (1f + (groundDrag * deltaTime)));
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
            currentVelocity *= (1f / (1f + (airDrag * deltaTime)));

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

            if (isSprinting)
            {
                isSprintJump = true;
            }


            // Change jump physics depending on the jump substate, reset yVel while adding to the return velocity
            if (isSprintJump)
            {
                // Sprint jumps accelerate the player forward to a limit, have less height, and have weaker gravity
                currentVelocity = CalcSprintJumpVelocity(currentVelocity);
            }
            else
            {
                currentVelocity = new Vector3(currentVelocity.x, jumpUpSpeed, currentVelocity.z);
            }


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
            if (IsGrounded())
            {
                currentVelocity += _internalVelocityAdd;
            }
            else
            {
                currentVelocity += 0.5f * _internalVelocityAdd;
            }
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
                if (isSprintJump)
                {
                    isSprintJump = false;
                }
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
        if (coll.CompareTag("PhaseableWallController"))
        {
            ColliderLink colliderLink = coll.gameObject.GetComponentInChildren<ColliderLink>();
            if (colliderLink != null)
            {
                if (isDashing && colliderLink.isDashable)
                {
                    return false;
                }
                else if (colliderLink.usesSoapstones && (GameManager.instance.soapstones >= colliderLink.soapstonesRequired))
                {
                    return false;
                }
            }
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

    public override Vector3 GetVelocity()
    {
        return Motor.Velocity;
    }

    public override void AddVelocity(Vector3 velocity)
    {
        _internalVelocityAdd += velocity;
    }

    public override void SetVelocity(Vector3 velocity)
    {
        setVelocity = true;
        _internalVelocitySet = velocity;
    }

    public void SetPosition(Vector3 position)
    {
        setPosition = true;
        _internalPositionSet = position;
    }

    public override void LockVelocity(Vector3 velocity)
    {
        velocityLocked = true;
        lockedVelocity = velocity;
    }

    public override void UnlockVelocity()
    {
        velocityLocked = false;
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    protected void OnLanded()
    {
        playerVFXManager.StartLandVFX(transform.position - Vector3.up / 2);
        if (cinemachineManager)
        {
            cinemachineManager.ScreenShake(0.25f, 1.5f);
        }
    }

    protected void OnLeaveStableGround()
    {
    }

    public void OnDiscreteCollisionDetected(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("PhaseableWallController"))
        {
            GameObject enemy = other.gameObject;


            // Note: phasing can be handled in this script, but should be controlled via method calls from the Dash State

            if (isDashing)
            { // Phase through gates, but only if dashing.
              //Debug.Log("DISABLINGCOLLISION");
                Physics.IgnoreCollision(
                    other,
                    GetComponent<Collider>(), true);
                //StartCoroutine(ReenableCollision(dashDuration - DashTimer, enemy.gameObject.GetComponent<Collider>()));
            }
            else
            { // Can't bypass
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

    private Vector3 CalcSprintJumpVelocity(Vector3 currentVelocity)
    {
        float newVelocityX = currentVelocity.x * 1.15f;
        float newVelocityY = currentVelocity.z * 1.15f;
        Vector2 newVelocity = new Vector2(newVelocityX, newVelocityY);
        if (newVelocity.magnitude > 0.9f * sprintSpeedMult * MaxStableMoveSpeed)
        {
            newVelocityX *= (0.925f * sprintSpeedMult * MaxStableMoveSpeed) / (newVelocity.magnitude);
            newVelocityY *= (0.925f * sprintSpeedMult * MaxStableMoveSpeed) / (newVelocity.magnitude);
        }

        // magnitude of new velocities is reduced to that of max stable move speed * 1.2

        return new Vector3(newVelocityX, jumpUpSpeed * 0.75f, newVelocityY);
    }

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
        playerVFXManager.StartJumpVFX(transform.position - Vector3.up / 2);
        if (cinemachineManager)
        {
            cinemachineManager.ScreenShake(0.5f, 1.25f);
        }
    }

    public void StopJump()
    {
        //Debug.Log("stopping jump");
        jumpHeld = false;
    }

    public override void ApplyImpulseForce(Vector3 direction, float power)
    {
        AddVelocity(direction.normalized * power);
    }

    public override void AddSpeedModifier(float modifier)
    {
        throw new NotImplementedException();
    }

    public override void RemoveSpeedModifier(float modifier)
    {
        throw new NotImplementedException();
    }

    public override bool IsGrounded()
    {
        return Motor.GroundingStatus.IsStableOnGround;
    }

    public bool IsAbleToJump()
    {
        return AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : IsGrounded() || timeSinceLastAbleToJump <= jumpPostGroundingGraceTime;
    }

    public override void SetAllowMovement(bool isAllowed)
    {
        canMove = isAllowed;
    }

    public override void SetGroundDrag(float drag)
    {
        groundDrag = drag;
    }

    public void SetAirDrag(float drag)
    {
        airDrag = drag;
    }

    public void ResetDrag()
    {
        airDrag = defaultAirDrag;
        groundDrag = defaultGroundDrag;
    }

    public override void SetAllowRotation(bool isAllowed)
    {
        canRotate = isAllowed;
    }

    // Called by external scripts to set rotation.
    public void RotateToDir(Vector3 dir)
    {
        if (canRotate && dir != Vector3.zero)
        {
            ignoreSmoothRotation = true;
            savedUpdatedRotation = Quaternion.LookRotation(dir, Motor.CharacterUp);
        }
    }

    public void TeleportTo(Vector3 pos)
    {
        Motor.SetPosition(pos);
    }

    public void SetSprinting(bool newSprintBool)
    {
        isSprinting = newSprintBool;
    }

    public bool GetSprinting()
    {
        return isSprinting;
    }

    public void SetSprintJump(bool newSprintJumpBool)
    {
        isSprintJump = newSprintJumpBool;
    }
    public bool GetSprintJump()
    {
        return isSprintJump;
    }
}