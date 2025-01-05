using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

public enum PlayerRotationState
{
    Default,
    Mouse,
    Locked
}


public struct PlayerCharacterInputs
{
    public float MoveAxisForward;
    public float MoveAxisRight;
    public Quaternion CameraRotation;
    public bool jumpPressed;
    public bool JumpHeld;
    public bool Dash;
}

public struct AICharacterInputs
{
    public Vector3 MoveVector;
    public Vector3 LookVector;
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

    [Header("Jumping")]
    public bool AllowJumpingWhenSliding = false;
    public float JumpUpSpeed = 10f;
    public float maxJumpTime = 2f;
    public float JumpScalableForwardSpeed = 10f;
    public float JumpPreGroundingGraceTime = 0f;
    public float JumpPostGroundingGraceTime = 0f;

    [Header("Misc")]
    public List<Collider> IgnoredColliders = new List<Collider>();
    public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
    public float BonusOrientationSharpness = 10f;
    public Vector3 Gravity = new Vector3(0, -30f, 0);
    public Transform MeshRoot;
    public Transform CameraFollowPoint;

    public PlayerRotationState CurrentRotationState { get; private set; }

    private Collider[] _probedColliders = new Collider[8];
    private RaycastHit[] _probedHits = new RaycastHit[8];
    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    private bool _jumpHeld = false;
    private bool _jumpPressedThisFrame = false;
    private bool _inJump = false;
    private bool _jumpedThisFrame = false;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;
    private float jumpTime = 0f;
    private Vector3 _internalVelocityAdd = Vector3.zero;

    private Vector3 lastInnerNormal = Vector3.zero;
    private Vector3 lastOuterNormal = Vector3.zero;

    private GameObject CharacterCamera;

    // Custom Variables Below:

        
    private RotationController rotationController;

    public bool isDashing = false;
    public ParticleSystem DashEffect;
    public float dashPower = 50f;
    [SerializeField] private float dashDuration = 0.35f;
    [HideInInspector] public float DashTimer = 0.0f;
    [SerializeField] private float DashCooldown = 0.6f;
    [HideInInspector] public float dashCooldownTimer = 0.0f;
    private bool dashOnCooldown = false;

    bool canMove = true;

    bool setVelocity = false;
    private Vector3 _internalVelocitySet = Vector3.zero;

    bool velocityLocked = false;
    private Vector3 lockedVelocity;
    private bool isFacingMouse = false;
    private Vector2 directionVec;
    private Quaternion savedLockedRotation;
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

        rotationController = gameObject.GetComponentInChildren<RotationController>();

    }

    private void Update() // Handles all movement related input.
    {
        if (_inJump) jumpTime += Time.deltaTime;

        if (DashTimer > 0) {
            DashTimer -= Time.deltaTime;
            if (DashTimer <= 0) {
                canMove = true;
                isDashing = false;
                UnlockVelocity();
                SetAllowRotation(true);
            }
        }

        if (dashCooldownTimer > 0) {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0) {
                dashOnCooldown = false;
            }

        }

            
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
            case PlayerRotationState.Mouse:
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
            case PlayerRotationState.Mouse:
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
    public void SetInputs(ref PlayerCharacterInputs playerInput)
    {
        PlayerCharacterInputs inputs = playerInput;

        inputs.CameraRotation = CharacterCamera.transform.rotation;

        // Clamp input
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        // Move and look inputs
        _moveInputVector = cameraPlanarRotation * moveInputVector;
        if (!canMove) {
            _moveInputVector = new Vector3(0, 0, 0);
        }

        _lookInputVector = cameraPlanarDirection;

        // Jumping input
        if (inputs.JumpHeld)
        {
            _timeSinceJumpRequested = 0f;
            _jumpHeld = true;
        }
        else
        {
            _jumpHeld = false;
        }

        if(inputs.jumpPressed)
        {
            _jumpPressedThisFrame = true;
        }

        if (inputs.Dash)
        {
            StartDash();
        }

    }

    /// <summary>
    /// This is called every frame by the AI script in order to tell the character what its inputs are
    /// </summary>
    public void SetInputs(ref AICharacterInputs inputs)
    {
        _moveInputVector = inputs.MoveVector;
        _lookInputVector = inputs.LookVector;
    }

    private Quaternion _tmpTransientRot;

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime)
    {
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
            case PlayerRotationState.Mouse:
                {
                    rotateToMouse();
                    break;
                }
            case PlayerRotationState.Default:
                {
                    /*if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
                    {
                        // Smoothly interpolate from current to target look direction
                        Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _moveInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                        // Set the current rotation (which will be used by the KinematicCharacterMotor)
                        transform.rotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                    }*/
                    rotateToMovementAngle();
                    break;
                }
        }

        // THIS IS TEMPORARY!!!!!
        // REPLACE THIS AS SOON AS STATES CAN BE READ FROM ACTION MANAGER
        if (CurrentRotationState == PlayerRotationState.Locked) {
            SetAllowRotation(true);
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
            Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
        }
        // Air movement
        else
        {
            // Add move input
            if (_moveInputVector.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = _moveInputVector * AirAccelerationSpeed * deltaTime;

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
            currentVelocity += Gravity * deltaTime;

            // Drag
            currentVelocity *= (1f / (1f + (Drag * deltaTime)));

        }

        // Handle jumping
        _jumpedThisFrame = false;
        _timeSinceJumpRequested += deltaTime;

        // Should only start a new jump if the button is pressed while on jumpable ground
        // If the player is already in a jump for < maxJumpTime, then can continue gaining height
        bool onJumpableGround = (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround); //|| _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime;        
        if ((_jumpPressedThisFrame && onJumpableGround) || (_jumpHeld && _inJump))
        {
            //if (_jumpPressedThisFrame && onJumpableGround) Debug.Log("Starting jump");

            // See if we are allowed to continue increaing jump height
            if (jumpTime < maxJumpTime)
            {
                _inJump = true;
                //Debug.Log("Jumping!");
                // Calculate jump direction before ungrounding
                Vector3 jumpDirection = Motor.CharacterUp;
                if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                {
                    jumpDirection = Motor.GroundingStatus.GroundNormal;
                }

                // Makes the character skip ground probing/snapping on its next update. 
                // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                Motor.ForceUnground();

                // Add to the return velocity and reset jump state
                currentVelocity = new Vector3(currentVelocity.x, JumpUpSpeed, currentVelocity.z); //- Vector3.Project(currentVelocity, Motor.CharacterUp);
                //currentVelocity += (_moveInputVector * JumpScalableForwardSpeed);
                _jumpedThisFrame = true;
            }
            else
            {
                //Debug.Log("Ending jump");
                _inJump = false;
            }
        }
        else
        {
            _inJump = false;
        }

        _jumpPressedThisFrame = false;

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
        if (_jumpHeld && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
        {
            _jumpHeld = false;
        }

        if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
        {
            // If we're on a ground surface, reset jumping values
            if (!_jumpedThisFrame)
            {
                jumpTime = 0f;
                _inJump = false;
            }
            _timeSinceLastAbleToJump = 0f;
        }
        else
        {
            // Keep track of time since we were last able to jump (for grace period)
            _timeSinceLastAbleToJump += deltaTime;
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


            if (isDashing) { // Phase through gates, but only if dashing.
            //Debug.Log("DISABLINGCOLLISION");
            Physics.IgnoreCollision(
                other,
                GetComponent<Collider>(), true);
                StartCoroutine(ReenableCollision(dashDuration - DashTimer, enemy.gameObject.GetComponent<Collider>()));
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






    // Custom Functions Below:






    public void StartDash() {
        if (isDashing || dashOnCooldown) 
        {
            return;
        }

        //combatController.bufferDash(); // Allows the dash animation to play
                                        // set flag in animator
        isDashing = true;

        Vector3 dashDirection;

        var hDashIntent = Input.GetAxisRaw("Horizontal");
        var vDashIntent = Input.GetAxisRaw("Vertical");

        SetAllowRotation(false);

        if (Mathf.Abs(hDashIntent) + Mathf.Abs(vDashIntent) == 0f)
        {
            rotateToMouse();

            float h = Input.mousePosition.x - Screen.width / 2;
            float v = Input.mousePosition.y - Screen.height / 2;
            Vector3 mouseDirection = new Vector3(h, 0, v);
            mouseDirection.Normalize();
            mouseDirection = Quaternion.Euler(0, -45, 0) * mouseDirection;

            dashDirection = mouseDirection;
        }
        else
        {
            dashDirection = _moveInputVector;
        }

        LockVelocity(dashDirection * dashPower);


        
        DashTimer = dashDuration;
        dashOnCooldown = true;
        dashCooldownTimer = DashCooldown;
        canMove = false;

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

        // else if isAllowed = true
        if (CurrentRotationState == PlayerRotationState.Locked) {
            if (isFacingMouse) {
                TransitionToState(PlayerRotationState.Mouse);
            } else {
                TransitionToState(PlayerRotationState.Default);
            }
        }
    }

    // If false (ex moving), rotation is set to follow the movement keys. If true, (ex using the bubble), Typhis faces the mouse at all times.
    public void SetMouseRotation(bool facingMouse)
    {
        if (facingMouse) {
            isFacingMouse = true;

            if (CurrentRotationState != PlayerRotationState.Locked) {
                TransitionToState(PlayerRotationState.Mouse);
            }
        }
        else {
            isFacingMouse = false;

            if (CurrentRotationState != PlayerRotationState.Locked) {
                TransitionToState(PlayerRotationState.Default);
            }
        }
    }

    // Used by outside functions to set rotation to the mouse point (ex during sword swings) and then lock it.
    public void rotateToMouse()
    {
        float h = Input.mousePosition.x - Screen.width / 2;
        float v = Input.mousePosition.y - Screen.height / 2;
        Vector3 mouseDirection = new Vector3(h, 0, v);
        mouseDirection.Normalize();
        mouseDirection = Quaternion.Euler(0, -135, 0) * mouseDirection;

        // Vector3 smoothedLookInputDirection = Vector3.Slerp(_lookInputVector, mouseDirection, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

        // Set the current rotation (which will be used by the KinematicCharacterMotor)
        transform.rotation = Quaternion.LookRotation(mouseDirection, Motor.CharacterUp);
    }

    private void rotateToMovementAngle()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
        directionVec = camForward * input.x + camRight * input.y;
        directionVec.Normalize();

        if ( Quaternion.Angle( transform.rotation, Quaternion.Euler(0, angle - 90 -45, 0) ) == 180f ) {
            angle -= 90;
            //Debug.Log ("1 frame Flip");
        }

        if (input.x != 0 || input.y != 0)
        {
            transform.rotation = Quaternion.Euler(0, angle - 90 -45, 0);
        }

        //SetAllowRotation(false)
    }

    public Vector3 GetMouseDirection() {
        Vector3 dir;
        if (GameManager.isControllerUsed) {
            dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            dir.Normalize();
        } else {
            float h = Input.mousePosition.x - Screen.width / 2;
            float v = Input.mousePosition.y - Screen.height / 2;
            dir = new Vector3(h, 0, v);
            dir.Normalize();
        }

        dir = Quaternion.Euler(0, -45+180, 0) * dir;
    return dir;
    }
}