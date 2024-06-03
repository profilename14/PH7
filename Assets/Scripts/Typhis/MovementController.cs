using PixelCrushers.DialogueSystem.UnityGUI;
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
    //[SerializeField] const float DEFAULT_SPEED = 14.0f;
    [SerializeField] float speed = 1.2f;

    [SerializeField] bool hideMouse = false;

    bool isControllerUsed = false;

    Rigidbody rigidbody;
    // Used to store what direction the player is moving in. Right/Up positive, Left/Down negative.
    float horizontal;
    float vertical;

    // This is used to temporarily store the player position before translating them.
    Vector3 position;

    // There are five possible phases the player can be in, each one modifies speed depending on how long in the phase the player is.

    private RotationController rotationController;
    private PlayerCombatController combatController;

    // Dashing Logic
    public bool isDashing = false;
    public float dashSpeed = 60;
    public bool MouseDash = false;

    [SerializeField] private float dashDuration = 0.6f;
    [HideInInspector] public float DashTimer = 0.0f;
    [SerializeField] private float DashCooldown = 0.6f;
    [SerializeField] public float dashCooldownTimer = 0.0f;
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

    public ParticleSystem DashEffect;

    private Vector3 camForward;
    private Vector3 camRight;


    // ADSR Variables:

    [SerializeField] private float AttackDuration = 0.2f;
    [SerializeField] private AnimationCurve Attack;
    private float attackTimer;

    [SerializeField] private float DecayDuration = 0.4f;
    [SerializeField] private AnimationCurve Decay;
    private float decayTimer;

    [SerializeField] private float SustainDuration = 1.0f;
    [SerializeField] private AnimationCurve Sustain;
    private float sustainTimer;

    [SerializeField] private float ReleaseDuration = 0.3f;
    [SerializeField] private AnimationCurve Release;
    private float releaseTimer;

    private enum Phase { Attack, Decay, Sustain, Release, None };
    [SerializeField] private Phase currentPhase = Phase.None;

    [SerializeField] private Vector3 lastMove; // Where the player was going last frame. Used for slight sliding

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
        Cursor.visible = !hideMouse;

        dashCooldownTimer -= Time.deltaTime;

        // This gargantuan function accurately get's the player's direction respecting stages for FixedUpdate's translations.
        GetMovementDirection();

        var cam = Camera.main;

        camForward = cam.transform.forward;
        camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();


        /*// DASH LOGIC:
        // Dash should probably be disabled during attacks.
        if (dashCooldownTimer >= 0)
        { // Decrease cooldown
        }*/

        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0)
        {
            combatController.bufferDash();
        }

    }

    // Fixed update is used for better compatibility and physics.
    // It is important the player's position is changed here for clipping reasons.
    void FixedUpdate()
    {
        // Set the player's velocity to zero. This is to prevent continuous knockback when an enemy runs into the player.
        //rigidbody.velocity = new Vector3(0, 0, 0);
        if (isDashing) { // Main logic:

            dashCooldownTimer = DashCooldown;

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
            // dashVelocity = dashDirection * dashSpeed * ;
            dashVelocity = dashDirection * dashSpeed * Dash.Evaluate(DashTimer / dashDuration);

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

        // Get the controls direction then multiply by ADSR state:

        Vector3 moveDir = (camForward * vertical) + (camRight * horizontal);

        if (GameManager.isControllerUsed) {
          Vector3 inputCont = new Vector3( Input.GetAxis("Horizontal"), 0,
                                           Input.GetAxis("Vertical"));

          if (inputCont.magnitude > 1) {
            inputCont.Normalize();
          }

          Vector3 input = camForward * inputCont.x + camRight * inputCont.y;

          moveDir = input;


        }
        else {
          moveDir.Normalize();
        }


        if (Mathf.Abs(moveDir.x) + Mathf.Abs(moveDir.z) > 0.0 ) { // If the player's moving
          if (currentPhase == Phase.None || currentPhase == Phase.Release) { // If the player stopped and is moving now
            currentPhase = Phase.Attack;
            attackTimer = 0;
          }

          lastMove = moveDir; // If the player is just normally moving
        }
        else if (currentPhase == Phase.Decay || currentPhase == Phase.Attack || currentPhase == Phase.Sustain) { // Player let go while moving
          if (ADSREnvelope() < 1) {
            lastMove = lastMove * ADSREnvelope(); // Let go just when starting attack = no slide
          }
          currentPhase = Phase.Release;
          releaseTimer = 0;
        }

        if (currentPhase == Phase.Release) {
          moveDir = lastMove;
        }

        moveDir = moveDir * ADSREnvelope();

        moveVelocity = new Vector3(moveDir.x, rigidbody.velocity.y, moveDir.z) * 15 * speed;

        // !!This part is responsible for all actual movement!!
        if (canMove) {
          rigidbody.velocity = moveVelocity + dashVelocity + knockbackVelocity;
        } else {
          rigidbody.velocity = dashVelocity + knockbackVelocity;
        }

    }





    // This LONG chain of if statement sets get player input for WASD, and it helps implement 2D ADSR smoothly.
    // Things get much more advanced when translating things to 2D movement, so this is a long process
    private void GetMovementDirection()
    {
        // REVISIT FOR CONTROLLER SUPPORT
        horizontal = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        vertical = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Collider enemyCollider = other.GetComponent<Collider>();
            if (isDashing)
            {
                //Physics.IgnoreLayerCollision(9,)
            }
            else
            {
                //Physics.IgnoreCollision(enemyCollider, GetComponent<Collider>(), false);
            }
        }

        if (other.CompareTag("PhaseableWallController"))
        {
            ColliderLink wall = other.GetComponent<ColliderLink>();
            bool dashable = wall.isDashable;
            bool phBased = wall.usesPH;
            float minPH = wall.minPH;
            float maxPH = wall.maxPH;
            //float playerPH = gameObject.GetComponent<PlayerStats>().ph;


            if (isDashing && dashable) { // Phase through gates, but only if dashing.
              //Debug.Log("DISABLINGCOLLISION");
              Physics.IgnoreCollision(
                wall.linkedCollider,
                GetComponent<Collider>(), true);
            } else if (phBased
                       //&& playerPH <= maxPH + 0.1f
                       /*&& playerPH >= minPH - 0.1f*/) {
                   //Debug.Log("DISABLINGCOLLISION");
               Physics.IgnoreCollision(
                 wall.linkedCollider,
                 GetComponent<Collider>(), true);
            } else { // Can't bypass
              //Debug.Log("ENABLINGCOLLISION");
              Physics.IgnoreCollision(
                wall.linkedCollider,
                GetComponent<Collider>(), false);
            }

        }

    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            GameObject enemy = other.gameObject;


            if (isDashing) { // Phase through gates, but only if dashing.
              //Debug.Log("DISABLINGCOLLISION");
              Physics.IgnoreCollision(
                enemy.gameObject.GetComponent<Collider>(),
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

    public void startDash()
    {
        //Debug.Log("Starting dash");

        //rotationController.snapToCurrentAngle();

        // Start dash
        isDashing = true;
        combatController.bufferDash(); // Allows the dash animation to play
                                       // set flag in animator
        if (!MouseDash)
        {
            var hDashIntent = horizontal;
            var vDashIntent = vertical;

            if (Mathf.Abs(hDashIntent) + Mathf.Abs(vDashIntent) == 0f)
            {
                dashDirection = camForward * rotationController.transform.forward.z
                              + camRight * rotationController.transform.forward.x;
                dashDirection = Quaternion.Euler(0, 90 + 45, 0) * dashDirection;
            }
            else
            {
                dashDirection = camForward * vDashIntent + camRight * hDashIntent;
                //dashDirection = rotationController.directionVec;
            }
        }
        else
        {
            //dashDirection = rotationController.directionVec;
            dashDirection = rotationController.directionVec;
        }
        dashVelocity = new Vector3(0, 0, 0);
        DashTimer = 0;
        canMove = false;
    }

    float ADSREnvelope()
    {
        float velocity = 0.0f;

        if (Phase.Attack == this.currentPhase)
        {
            velocity = this.Attack.Evaluate(this.attackTimer / this.AttackDuration);
            this.attackTimer += Time.deltaTime;
            if (this.attackTimer > this.AttackDuration)
            {
                this.currentPhase = Phase.Decay;
                decayTimer = 0;
            }
        }
        else if (Phase.Decay == this.currentPhase)
        {
            velocity = this.Decay.Evaluate(this.decayTimer / this.DecayDuration);
            this.decayTimer += Time.deltaTime;
            if (this.decayTimer > this.DecayDuration)
            {
                this.currentPhase = Phase.Sustain;
                sustainTimer = 0;
            }
        }
        else if (Phase.Sustain == this.currentPhase)
        {
            velocity = this.Sustain.Evaluate(this.sustainTimer / this.SustainDuration);
            this.sustainTimer += Time.deltaTime;
        }
        else if (Phase.Release == this.currentPhase)
        {
            velocity = this.Release.Evaluate(this.releaseTimer / this.ReleaseDuration);
            this.releaseTimer += Time.deltaTime;
            if (this.releaseTimer > this.ReleaseDuration)
            {
                this.currentPhase = Phase.None;
            }
        }
        return velocity;
    }

}
