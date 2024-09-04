using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using PixelCrushers.DialogueSystem;
using UnityEditor.Experimental.GraphView;

namespace KinematicCharacterController.Examples
{
    public enum CharacterState
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
        public bool JumpDown;
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

    public class ExampleCharacterController : MonoBehaviour, ICharacterController
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

        public CharacterState CurrentRotationState { get; private set; }

        private Collider[] _probedColliders = new Collider[8];
        private RaycastHit[] _probedHits = new RaycastHit[8];
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private bool _jumpRequested = false;
        private bool _jumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;
        private Vector3 _internalVelocityAdd = Vector3.zero;

        private Vector3 lastInnerNormal = Vector3.zero;
        private Vector3 lastOuterNormal = Vector3.zero;

        private GameObject CharacterCamera;

        // Custom Variables Below:

        
        private RotationController rotationController;
        private PlayerCombatController combatController;

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

        private void Awake()
        {
            // Handle initial state
            TransitionToState(CharacterState.Default);

            // Assign the characterController to the motor
            Motor.CharacterController = this;

            CharacterCamera = GameObject.FindGameObjectWithTag("MainCamera");

            rotationController = gameObject.GetComponentInChildren<RotationController>();

            combatController = gameObject.GetComponentInChildren<PlayerCombatController>();
        }

        private void Update() // Handles all movement related input.
        {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            // Build the CharacterInputs struct
            characterInputs.MoveAxisForward = Input.GetAxisRaw("Vertical");
            characterInputs.MoveAxisRight = Input.GetAxisRaw("Horizontal");
            characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Q);
            characterInputs.CameraRotation = CharacterCamera.transform.rotation;
            characterInputs.Dash = Input.GetKeyDown(KeyCode.Space);

            // Apply inputs to character
            SetInputs(ref characterInputs);

            if (DashTimer > 0) {
                DashTimer -= Time.deltaTime;
                if (DashTimer <= 0) {
                    canMove = true;
                    isDashing = false;
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
        public void TransitionToState(CharacterState newState)
        {
            CharacterState tmpInitialState = CurrentRotationState;
            OnStateExit(tmpInitialState, newState);
            CurrentRotationState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        /// <summary>
        /// Event when entering a state
        /// </summary>
        public void OnStateEnter(CharacterState state, CharacterState fromState)
        {
            switch (state)
            {
                case CharacterState.Default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Event when exiting a state
        /// </summary>
        public void OnStateExit(CharacterState state, CharacterState toState)
        {
            switch (state)
            {
                case CharacterState.Default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
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
            if (inputs.JumpDown)
            {
                _timeSinceJumpRequested = 0f;
                _jumpRequested = true;
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
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // This function is disabled for now as reotation controller already works. However, if we ever move past said script,
            // there's a partial implementation here for rotation being done along movement.

            /*
            switch (CurrentRotationState)
            {
                case CharacterState.Locked:
                    {
                        break;
                    }
                case CharacterState.Mouse:
                    {
                        float h = Input.mousePosition.x - Screen.width / 2;
                        float v = Input.mousePosition.y - Screen.height / 2;
                        Vector3 mouseDirection = new Vector3(h, 0, v);
                        mouseDirection.Normalize();
                        mouseDirection = Quaternion.Euler(0, -45, 0) * mouseDirection;

                        //Vector3 smoothedLookInputDirection = Vector3.Slerp(_lookInputVector, mouseDirection, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                        // Set the current rotation (which will be used by the KinematicCharacterMotor)
                        currentRotation = Quaternion.LookRotation(mouseDirection, Motor.CharacterUp);
                        break;
                    }
                case CharacterState.Default:
                    {
                        if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
                        {
                            // Smoothly interpolate from current to target look direction
                            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _moveInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                            // Set the current rotation (which will be used by the KinematicCharacterMotor)
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                        }

                        Vector3 currentUp = (currentRotation * Vector3.up);
                        if (BonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                        {
                            // Rotate from current up to invert gravity
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                        }
                        else if (BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                        {
                            if (Motor.GroundingStatus.IsStableOnGround)
                            {
                                Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);

                                Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

                                // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                                Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
                            }
                            else
                            {
                                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                            }
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                        }
                        break;
                    }
            }
            */
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
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

                // Gravity
                currentVelocity += Gravity * deltaTime;

                // Drag
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));

            }

            // Handle jumping
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            if (_jumpRequested)
            {
                // See if we actually are allowed to jump
                if (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
                {
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
                    currentVelocity += (jumpDirection * JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                    currentVelocity += (_moveInputVector * JumpScalableForwardSpeed);
                    _jumpRequested = false;
                    _jumpConsumed = true;
                    _jumpedThisFrame = true;
                }
            }

            // Take into account additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }

            if (setVelocity) {
                setVelocity = false;
                currentVelocity = _internalVelocitySet;
                _internalVelocitySet = Vector3.zero;
            }

        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {

            // Handle jumping pre-ground grace period
            if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
            {
                _jumpRequested = false;
            }

            if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
            {
                // If we're on a ground surface, reset jumping values
                if (!_jumpedThisFrame)
                {
                    _jumpConsumed = false;
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

            rotationController.snapToCurrentMouseAngle();
            combatController.bufferDash(); // Allows the dash animation to play
                                           // set flag in animator
            isDashing = true;

            Vector3 dashDirection;

            var hDashIntent = Input.GetAxisRaw("Horizontal");
            var vDashIntent = Input.GetAxisRaw("Vertical");

            if (Mathf.Abs(hDashIntent) + Mathf.Abs(vDashIntent) == 0f)
            {
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

            SetVelocity(dashDirection * dashPower);


        
            DashTimer = dashDuration;
            dashOnCooldown = true;
            dashCooldownTimer = DashCooldown;
            canMove = false;

        }

        public void applyKnockback(Vector3 source, float power) 
        { // !!!Change name to applyForce

            Vector3 direction = (transform.position - source);
            direction.Normalize();

            direction = direction * power * 30;

            AddVelocity(-direction);
        }

        
    }


}