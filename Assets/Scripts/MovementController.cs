using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Made by Jasper Fadden
// This is an updated movement controller for Typhis, adapted from CrunchTime's movement code.
// This is only for movement: Everything else in the player controller is elsewhere.

// Credits to Josh McCoy for much of the code implementing Attack-Decay-Sustain-Release movement.

public class MovementController : MonoBehaviour
{
    // Movement speed for the player.
    const float DEFAULT_SPEED = 11.0f;
    float speed = DEFAULT_SPEED;

    bool isControllerUsed = false;

    Rigidbody rigidbody;
    // Used to store what direction the player is moving in. Right/Up positive, Left/Down negative.
    float horizontal;
    float vertical;
    // This is used to temporarily store the player position before translating them.
    Vector3 position;


    // To avoid counter-strike style bunnyhop shennanigans, decay and sustain are basically ignored and set to normal speed.
    [SerializeField] private float AttackDuration = 0.45f;
    [SerializeField] private AnimationCurve Attack;

    [SerializeField] private float DecayDuration = 1.0f;
    [SerializeField] private AnimationCurve Decay;

    [SerializeField] private float SustainDuration = 1.0f;
    [SerializeField] private AnimationCurve Sustain;

    [SerializeField] private float ReleaseDuration = 0.64f;
    [SerializeField] private AnimationCurve Release;

    // Two versions must be made for most variables in order to bring the code from 1D to 2D
    private float HAttackTimer;
    private float HDecayTimer;
    private float HSustainTimer;
    private float HReleaseTimer;

    private float VAttackTimer;
    private float VDecayTimer;
    private float VSustainTimer;
    private float VReleaseTimer;

    // These custom made timers are to prevent the player from sliding when they stop themself by pressing contrasting movement keys
    private float HDualReleaseTimer = 0.0f;
    private float VDualReleaseTimer = 0.0f;
    private float DualReleaseThreshold = 0.1f;

    // There are five possible phases the player can be in, each one modifies speed depending on how long in the phase the player is.
    private enum Phase { Attack, Decay, Sustain, Release, None };
    private Phase CurrentPhaseVertical;
    private Phase CurrentPhaseHorizontal;

    private RotationController rotationController;
    private PlayerCombatController combatController;

    // Dashing Logic
    public bool isDashing = false;
    public float dashSpeed = 60;

    [SerializeField] private float dashDuration = 0.6f;
    private float DashTimer = 0.0f;
    [SerializeField] private float DashCooldown = 0.6f;
    private float dashCooldownTimer = 0.0f;
    private Vector3 dashDirection;

    [SerializeField] private AnimationCurve Dash;

    // This % through, the player will no longer deal pH damage but will be able to move,
    // still be invulnerable, and still have momentum.
    // At this point the dash animation starts to end and the player is soon also able to attack.
    [SerializeField] private float DashAftermathPercent = 0.80f;
    public bool dashEnding = false;
    private bool canMove = true;


    private float knockbackDuration = 0.6f; // Yes I'm actually programming knockback with dash code
    private float knockbackTimer = 0.0f;    // Kinda forced to deal with hand coding this do to ADSR movement
    private float knockbackPower = 0.0f;    // Oof
    private Vector3 knockbackSource;
    public bool isBeingKnockedBack = false;
    [SerializeField] private AnimationCurve Knockback;
    public float slowdownWhileAttacking = 2;


    // All velocities in fixedUpdate are stored before they're all applied at once.
    private Vector3 moveVelocity;
    private Vector3 dashVelocity;
    private Vector3 knockbackVelocity;

    private Vector3 controllerMovement;


    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

        rotationController = gameObject.GetComponentInChildren<RotationController>();

        combatController = gameObject.GetComponentInChildren<PlayerCombatController>();

        moveVelocity = new Vector3(0, 0, 0);
        dashVelocity = new Vector3(0, 0, 0);
        knockbackVelocity = new Vector3(0, 0, 0);
    }

    // Update is called once per frame.
    void Update()
    {
        DecrementTimers(); // Does not move the player, but prepares fixed update to

        // This gargantuan function accurately get's the player's direction respecting stages for FixedUpdate's translations.
        GetMovementDirection();


        // DASH LOGIC:

        // Dash should probably be disabled during attacks.

        if (dashCooldownTimer >= 0) { // Decrease cooldown
          dashCooldownTimer -= Time.deltaTime;
        }

        if ( (Input.GetKeyDown("space") || Input.GetButton("Jump") ) && !isDashing && dashCooldownTimer <= 0
            && rotationController.canTurn ) { // Start dash
          isDashing = true;
          // set flag in animator
          dashDirection = rotationController.directionVec;
          dashVelocity = new Vector3(0, 0, 0);
          DashTimer = 0;
          canMove = false;
        }




    }

    // Fixed update is used for better compatibility and physics.
    // It is important the player's position is changed here for clipping reasons.
    void FixedUpdate()
    {
        // Set the player's velocity to zero. This is to prevent continuous knockback when an enemy runs into the player.
        //rigidbody.velocity = new Vector3(0, 0, 0);

        if (isDashing) { // Main logic:

          if (DashTimer >= dashDuration * (DashAftermathPercent) && dashEnding == false) {
            dashEnding = true;
            canMove = true;
            // Set animator to wrap up dash animation
          }
          if (DashTimer >= dashDuration) {
            DashTimer = 0;
            isDashing = false;
            dashCooldownTimer = DashCooldown;
            dashEnding = false;
            dashVelocity = new Vector3(0, 0, 0);

          } else {
            // Set the velocity according the the graph curve
            float curDashVelocity = Dash.Evaluate(DashTimer / dashDuration);
            dashVelocity = dashDirection * curDashVelocity * dashSpeed;

            DashTimer += Time.deltaTime;
          }
        }

        if (isBeingKnockedBack) {
          if (knockbackTimer >= knockbackDuration) {
            knockbackTimer = 0;
            isBeingKnockedBack = false;
            knockbackVelocity = new Vector3(0, 0, 0);
          } else {
            float curKnockbackVelocity = Knockback.Evaluate(knockbackTimer / knockbackDuration);
            Vector3 direction = -((knockbackSource - transform.position).normalized);
            knockbackVelocity = direction * curKnockbackVelocity * knockbackPower;

            knockbackTimer += Time.deltaTime;
          }

        }

        if (combatController.isAttacking()) {
          speed = DEFAULT_SPEED / slowdownWhileAttacking;
        } else {
          speed = DEFAULT_SPEED;
        }

        if (GameManager.isControllerUsed) {



          Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

          if (input.magnitude > 1) {
            //input.Normalize();
          }

          Vector3 movement = new Vector3(input.x * speed * Time.deltaTime * 1.1f, 0, input.z * speed * Time.deltaTime * 1.1f);

          // We can polish this later
          controllerMovement = movement;

        }

        position = this.gameObject.transform.position;

        if (this.CurrentPhaseHorizontal != Phase.None)
        {
            // Note position.x is changed based on "horizontal." The speed is decreased for diagonal movement.
            if (vertical != 0 || true)
            {
                position.x += horizontal * speed * HorizontalADSREnvelope() * Time.deltaTime * (1.0f / (1 + 0.4142f * Mathf.Abs(vertical)) );
            }
            else
            {
                position.x += horizontal * speed * HorizontalADSREnvelope() * Time.deltaTime;
            }

        }

        if (this.CurrentPhaseVertical != Phase.None)
        {
            // Note position.y is changed based on "vertical." Again, speed is decreased for diagonal movement.
            if (horizontal != 0 || true)
            {
                position.z += vertical * speed * VerticalADSREnvelope() * Time.deltaTime * (1.0f / (1 + 0.4142f * Mathf.Abs(horizontal)) );
            }
            else
            {
                position.z += vertical * speed * VerticalADSREnvelope() * Time.deltaTime;
            }
        }


        moveVelocity = (position - transform.position) * 50;

        // Floats are often error ridden and slightly off, so this code ensure's the player is always properly stopped when required.
        if (horizontal < 0.01 && horizontal > -0.01)
        {
            horizontal = 0;
        }
        if (vertical < 0.01 && vertical > -0.01)
        {
            vertical = 0;
        }

        if (GameManager.isControllerUsed) {
          moveVelocity = controllerMovement * 50;
        }


        // !!This part is responsible for all actual movement!!
        if (canMove) {
          rigidbody.velocity = moveVelocity + dashVelocity + knockbackVelocity;
        } else {
          rigidbody.velocity = dashVelocity + knockbackVelocity;
        }




    }

    // Simple function borrowed from JoshMcCoy, these resets all ADSR timers.
    private void ResetTimersHorizontal()
    {
        this.HAttackTimer = 0.0f;
        this.HDecayTimer = 0.0f;
        this.HSustainTimer = 0.0f;
        this.HReleaseTimer = 0.0f;
    }
    private void ResetTimersVertical()
    {

        this.VAttackTimer = 0.0f;
        this.VDecayTimer = 0.0f;
        this.VSustainTimer = 0.0f;
        this.VReleaseTimer = 0.0f;
    }

    // Code from JoshMcCoy, this essentially allows us to modify the value of movement depending on current state.
    // Two versions exist for horizontal and vertical movement.
    float HorizontalADSREnvelope()
    {
        float velocity = 0.0f;

        if (Phase.Attack == this.CurrentPhaseHorizontal)
        {
            velocity = this.Attack.Evaluate(this.HAttackTimer / this.AttackDuration);
            this.HAttackTimer += Time.deltaTime;
            if (this.HAttackTimer > this.AttackDuration)
            {
                this.CurrentPhaseHorizontal = Phase.Decay;
            }
        }
        else if (Phase.Decay == this.CurrentPhaseHorizontal)
        {
            velocity = this.Decay.Evaluate(this.HDecayTimer / this.DecayDuration);
            this.HDecayTimer += Time.deltaTime;
            if (this.HDecayTimer > this.DecayDuration)
            {
                this.CurrentPhaseHorizontal = Phase.Sustain;
            }
        }
        else if (Phase.Sustain == this.CurrentPhaseHorizontal)
        {
            velocity = this.Sustain.Evaluate(this.HSustainTimer / this.SustainDuration);
            this.HSustainTimer += Time.deltaTime;
        }
        else if (Phase.Release == this.CurrentPhaseHorizontal)
        {
            velocity = this.Release.Evaluate(this.HReleaseTimer / this.ReleaseDuration);
            this.HReleaseTimer += Time.deltaTime;
            if (this.HReleaseTimer > this.ReleaseDuration)
            {
                this.CurrentPhaseHorizontal = Phase.None;
            }
        }
        return velocity;
    }

    // Vertical version.
    float VerticalADSREnvelope()
    {
        float velocity = 0.0f;

        if (Phase.Attack == this.CurrentPhaseVertical)
        {
            velocity = this.Attack.Evaluate(this.VAttackTimer / this.AttackDuration);
            this.VAttackTimer += Time.deltaTime;
            if (this.VAttackTimer > this.AttackDuration)
            {
                this.CurrentPhaseVertical = Phase.Decay;
            }
        }
        else if (Phase.Decay == this.CurrentPhaseVertical)
        {
            velocity = this.Decay.Evaluate(this.VDecayTimer / this.DecayDuration);
            this.VDecayTimer += Time.deltaTime;
            if (this.VDecayTimer > this.DecayDuration)
            {
                this.CurrentPhaseVertical = Phase.Sustain;
            }
        }
        else if (Phase.Sustain == this.CurrentPhaseVertical)
        {
            velocity = this.Sustain.Evaluate(this.VSustainTimer / this.SustainDuration);
            this.VSustainTimer += Time.deltaTime;
        }
        else if (Phase.Release == this.CurrentPhaseVertical)
        {
            velocity = this.Release.Evaluate(this.VReleaseTimer / this.ReleaseDuration);
            this.VReleaseTimer += Time.deltaTime;
            if (this.VReleaseTimer > this.ReleaseDuration)
            {
                this.CurrentPhaseVertical = Phase.None;
            }
        }
        return velocity;
    }

    // To avoid crowding Update() more than it is, this function decrements all timers.
    private void DecrementTimers()
    {
        // These timers count down every frame if they are above 0
        // These "dual release timers" are designed to allow the player to stop on the spot if they hit the reverse direction
        // and release both related movement keys. Otherwise, the player would bounce in a random direction after releasing both.
        if (HDualReleaseTimer > 0)
        {
            HDualReleaseTimer -= Time.deltaTime;
        }
        if (VDualReleaseTimer > 0)
        {
            VDualReleaseTimer -= Time.deltaTime;
        }
    }

    // This LONG chain of if statement sets get player input for WASD, and it helps implement 2D ADSR smoothly.
    // Things get much more advanced when translating things to 2D movement, so this is a long process
    private void GetMovementDirection()
    {
        // REVISIT FOR CONTROLLER SUPPORT

        // These are the 4 GetKeyDown Statements for the attack phase.
        // Note that directions and phases aren't updated every frame, only on keydown or keyup mostly.
        // This is to allow time based progression of states in case we decide to use the middle two stages.
        if (Input.GetKeyDown(KeyCode.D))
        {
            horizontal = 1.0f;
            this.ResetTimersHorizontal();
            this.CurrentPhaseHorizontal = Phase.Attack;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            horizontal = -1.0f;
            this.ResetTimersHorizontal();
            this.CurrentPhaseHorizontal = Phase.Attack;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            vertical = 1.0f;
            this.ResetTimersVertical();
            this.CurrentPhaseVertical = Phase.Attack;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            vertical = -1.0f;
            this.ResetTimersVertical();
            this.CurrentPhaseVertical = Phase.Attack;
        }

        // These are a set of four, complex release statements for each key. The first is described in detail.
        // This first loop only works if it has been enough time since the other horizontal key was pressed,
        // the player was in attack phase for long enough to speed up to full, and the right key was released.
        if (Input.GetKeyUp(KeyCode.D) && HDualReleaseTimer <= 0 && this.HAttackTimer > this.AttackDuration)
        {
            horizontal = 1.0f;
            this.CurrentPhaseHorizontal = Phase.Release;
        }
        // The second loop doesn't require the the attack phase was finished (as in, the player doesn't have to fully accelerate).
        else if (Input.GetKeyUp(KeyCode.D) && HDualReleaseTimer <= 0)
        {
            // If so, their sliding speed is scaled down, so just tapping the button doesn't result in a huge sliding boost.
            horizontal = this.HAttackTimer / this.AttackDuration;
            this.CurrentPhaseHorizontal = Phase.Release;
            // This switches the current direction if the player let's go of one of the keys after holding both
            if (Input.GetKey(KeyCode.A))
            {
                horizontal = -1.0f;
                this.CurrentPhaseHorizontal = Phase.Attack;
            }
        }
        // If the "HDualReleaseTimer," then the other button was recently held. This means the player shouldn't slide at all.
        else if (Input.GetKeyUp(KeyCode.D))
        {
            this.CurrentPhaseHorizontal = Phase.None;
            // This switches the current direction if the player let's go of one of the keys after holding both
            if (Input.GetKey(KeyCode.A))
            {
                horizontal = -1.0f;
                this.CurrentPhaseHorizontal = Phase.Attack;
            }
        }

        if (Input.GetKeyUp(KeyCode.A) && HDualReleaseTimer <= 0 && this.HAttackTimer > this.AttackDuration)
        {
            horizontal = -1.0f;
            this.CurrentPhaseHorizontal = Phase.Release;
        }
        else if (Input.GetKeyUp(KeyCode.A) && HDualReleaseTimer <= 0)
        {
            horizontal = -1.0f * this.HAttackTimer / this.AttackDuration;
            this.CurrentPhaseHorizontal = Phase.Release;

            if (Input.GetKey(KeyCode.D))
            {
                horizontal = 1.0f;
                this.CurrentPhaseHorizontal = Phase.Attack;
            }
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            this.CurrentPhaseHorizontal = Phase.None;

            if (Input.GetKey(KeyCode.D))
            {
                horizontal = 1.0f;
                this.CurrentPhaseHorizontal = Phase.Attack;
            }
        }

        if (Input.GetKeyUp(KeyCode.W) && VDualReleaseTimer <= 0 && this.VAttackTimer > this.AttackDuration)
        {
            vertical = 1.0f;
            this.CurrentPhaseVertical = Phase.Release;
        }
        else if (Input.GetKeyUp(KeyCode.W) && VDualReleaseTimer <= 0)
        {
            vertical = this.VAttackTimer / this.AttackDuration;
            this.CurrentPhaseVertical = Phase.Release;

            if (Input.GetKey(KeyCode.S))
            {
                vertical = -1.0f;
                this.CurrentPhaseVertical = Phase.Attack;
            }
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            this.CurrentPhaseVertical = Phase.None;

            if (Input.GetKey(KeyCode.S))
            {
                vertical = -1.0f;
                this.CurrentPhaseVertical = Phase.Attack;
            }
        }

        if (Input.GetKeyUp(KeyCode.S) && VDualReleaseTimer <= 0 && this.VAttackTimer > this.AttackDuration)
        {
            vertical = -1.0f;
            this.CurrentPhaseVertical = Phase.Release;
        }
        else if (Input.GetKeyUp(KeyCode.S) && VDualReleaseTimer <= 0)
        {
            vertical = -1.0f * this.VAttackTimer / this.AttackDuration;
            this.CurrentPhaseVertical = Phase.Release;

            if (Input.GetKey(KeyCode.W))
            {
                vertical = 1.0f;
                this.CurrentPhaseVertical = Phase.Attack;
            }
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            this.CurrentPhaseVertical = Phase.None;

            if (Input.GetKey(KeyCode.W))
            {
                vertical = 1.0f;
                this.CurrentPhaseVertical = Phase.Attack;
            }
        }


        // These 2 statements scan for no movement. Nested for clarity, as they only set
        // movement directions to 0 if the player is not currently sliding from the release phase.
        if (this.CurrentPhaseHorizontal == Phase.None)
        {
            //horizontal = 0.0f;
            if (!(Input.GetKey(KeyCode.A)) && !(Input.GetKey(KeyCode.D)))
            {
                horizontal = 0.0f;

            }
        }
        if (this.CurrentPhaseVertical == Phase.None)
        {
            //vertical = 0.0f;
            if (!(Input.GetKey(KeyCode.S)) && !(Input.GetKey(KeyCode.W)))
            {
                vertical = 0.0f;
            }
        }

        // These 2 statements scan for when the player is pressing both keys at once and set the player to
        // stop and not slide in the release phase (via the DualReleaseTimer system)
        if ((Input.GetKey(KeyCode.A)) && (Input.GetKey(KeyCode.D)))
        {
            horizontal /= -3;
            this.ResetTimersHorizontal();
            this.CurrentPhaseHorizontal = Phase.Sustain;
            // Set the dual release timer to a value so that the player doesn't bounce in a random direction after releasing both keys.
            HDualReleaseTimer = DualReleaseThreshold;
        }
        if ((Input.GetKey(KeyCode.S)) && (Input.GetKey(KeyCode.W)))
        {
            vertical /= -3;
            this.ResetTimersVertical();
            this.CurrentPhaseVertical = Phase.Sustain;
            VDualReleaseTimer = DualReleaseThreshold;
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.CompareTag("PhaseableWallController"))
        {
            bool dashable = other.GetComponent<ColliderLink>().isDashable;
            bool phBased = other.GetComponent<ColliderLink>().usesPH;
            float minPH = other.GetComponent<ColliderLink>().minPH;
            float maxPH = other.GetComponent<ColliderLink>().maxPH;
            float playerPH = gameObject.GetComponent<PlayerStats>().ph;

            if (isDashing && dashable) { // Phase through gates, but only if dashing.
              //Debug.Log("DISABLINGCOLLISION");
              Physics.IgnoreCollision(
                other.gameObject.GetComponent<ColliderLink>().linkedCollider,
                GetComponent<Collider>(), true);
            } else if (phBased
                       && playerPH < maxPH
                       && playerPH > minPH) {
                   //Debug.Log("DISABLINGCOLLISION");
               Physics.IgnoreCollision(
                 other.gameObject.GetComponent<ColliderLink>().linkedCollider,
                 GetComponent<Collider>(), true);
            } else { // Can't bypass
              //Debug.Log("ENABLINGCOLLISION");
              Physics.IgnoreCollision(
                other.gameObject.GetComponent<ColliderLink>().linkedCollider,
                GetComponent<Collider>(), false);
            }

        }
    }

    // Call this with an origin transform.position to push Typhis around
    // a power of 4 is probably strong enough for an explosion, 0.5 perhaps a scuttler.
    public void applyKnockback(Vector3 source, float power) {
      // Big limitation: only one knockback source at once. Should we reset?
      // if we JUST got exploded and then immediately snipped by a crab, then no
      if (isBeingKnockedBack) {
        float curKnockbackVelocity = Knockback.Evaluate(knockbackTimer / knockbackDuration);
        if (knockbackPower * curKnockbackVelocity > power) { // if old x 0-100% > new
          Debug.Log(knockbackPower * curKnockbackVelocity +" vs " + power + "w/" + curKnockbackVelocity);
          return;
        }
      }
      knockbackSource = source;
      isBeingKnockedBack = true;
      knockbackPower = power * 10;
      knockbackVelocity = new Vector3(0, 0, 0);
      knockbackTimer = 0;
    }

}
