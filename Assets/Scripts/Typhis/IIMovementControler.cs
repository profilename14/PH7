using PixelCrushers.DialogueSystem.UnityGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Made by Jasper Fadden
// This is an updated movement controller for Typhis, adapted from CrunchTime's movement code.
// This is only for movement: Everything else in the player controller is elsewhere.

// Credits to Josh McCoy for much of the code implementing Attack-Decay-Sustain-Release movement.

public class IIMovementController : MonoBehaviour
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

    // There are five possible phases the player can be in, each one modifies speed depending on how long in the phase the player is.

    private RotationController rotationController;
    private PlayerCombatController combatController;

    // Dashing Logic
    public bool isDashing = false;
    public float dashSpeed = 60;
    public bool MouseDash = true;

    [SerializeField] private float dashDuration = 0.6f;
    private float DashTimer = 0.0f;
    [SerializeField] private float DashCooldown = 0.6f;
    private float dashCooldownTimer = 0.0f;
    private Vector3 dashDirection;

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
        // This gargantuan function accurately get's the player's direction respecting stages for FixedUpdate's translations.
        GetMovementDirection();

        var cam = Camera.main;

        camForward = cam.transform.forward;
        camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        // DASH LOGIC:
        // Dash should probably be disabled during attacks.
        if (dashCooldownTimer >= 0)
        { // Decrease cooldown
            dashCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown("space") && !isDashing && dashCooldownTimer <= 0
            && rotationController.canTurn)
        { // Start dash
            isDashing = true;
            // set flag in animator
            if (!MouseDash)
            {
                var hDashIntent = horizontal;
                var vDashIntent = vertical;

                if (hDashIntent + vDashIntent == 0f)
                {
                    //dashDirection = rotationController.directionVec;
                }
                else
                {
                    dashDirection = camForward * vDashIntent + camRight * hDashIntent;
                }
            }
            else
            {
                //dashDirection = rotationController.directionVec;
            }
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
            dashVelocity = dashDirection * dashSpeed;

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

        //if (combatController.isAttacking()) {
        //  speed = DEFAULT_SPEED / slowdownWhileAttacking;
        //} else {
        //  speed = DEFAULT_SPEED;
        //}

        if (GameManager.isControllerUsed) {
          Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

          if (input.magnitude > 1) {
            //input.Normalize();
          }

          Vector3 movement = new Vector3(input.x * speed * Time.deltaTime * 1.1f, 0, input.z * speed * Time.deltaTime * 1.1f);

          // We can polish this later
          transform.position += movement;

          return;
        }

        Vector3 moveDir = camForward * vertical + camRight * horizontal;

        // Floats are often error ridden and slightly off, so this code ensure's the player is always properly stopped when required.
        if (horizontal < 0.01 && horizontal > -0.01)
        {
            horizontal = 0;
        }
        if (vertical < 0.01 && vertical > -0.01)
        {
            vertical = 0;
        }

        position = this.gameObject.transform.position;

        // Note position.x is changed based on "horizontal." The speed is decreased for diagonal movement.
        if (vertical != 0 || true)
        {
            position.x += horizontal * speed * Time.deltaTime * (1.0f / (1 + 0.4142f * Mathf.Abs(vertical)));
        }
        else
        {
            position.x += horizontal * speed * Time.deltaTime;
        }
        // Note position.y is changed based on "vertical." Again, speed is decreased for diagonal movement.
        if (horizontal != 0 || true)
        {
            position.z += vertical * speed * Time.deltaTime * (1.0f / (1 + 0.4142f * Mathf.Abs(horizontal)) );
        }
        else
        {
            position.z += vertical * speed * Time.deltaTime;
        }

        moveVelocity = new Vector3(moveDir.x, rigidbody.velocity.y, moveDir.z) * 10;

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
