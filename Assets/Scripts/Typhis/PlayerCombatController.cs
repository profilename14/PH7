using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    Animator playerAnim;
    RotationController rotationController;
    MovementController movementController;
    [SerializeField] AudioSource soundEffects;

    [Header("STATS")]
    public float health;
    public int ph;

    [Header("ATTACKS")]
    public float swing1Damage;
    public float swing1Knockback;
    public float swing2Damage;
    public float swing2Knockback;
    public float swing3Damage;
    public float swing3Knockback;

    //Is the player doing a left swing?
    private bool swingingL;


    //Is the player in interruptible recovery frames of an attack animation?
    private bool inRecovery;

    //Is the player in the idle animation state?
    public static bool isIdle;

    private bool inSwing;

    [HideInInspector]
    public bool inThrust;

    public bool inDash = true;

    private Vector3 rotForThrust;

    //Time left click needs to be held to initiate a thrust attack.
    [SerializeField]
    private float thrustHoldTime = 0.5f;

    private float holdTimer;

    private bool hasClicked = false;

    public enum PlayerState { Idle, Swing1, Swing2, Swing3, Dash }

    public static PlayerState currentState;

    //public static bool inRecoveryPublic;

    [SerializeField] private float waveSpellSpreadDegrees;
    [SerializeField] private GameObject waveSpellPrefab;
    [SerializeField] private float waveSpellCooldown = 0.42f;
    private float castTimer = 0f;


    [SerializeField] private GameObject telekinesisSpellPrefab;
    [SerializeField] private float telekinesisSpellCooldown = 2.5f;
    [HideInInspector] public float telekinesisCastTimer = 0f;

    public bool isFacingMouse = false;

    [SerializeField]
    private bool thrustIsDashAttack = true;

    public float timeToComboReset = 0.3f;

    private float comboResetTimer;

    public int lastSwingNum;

    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();
        rotationController = transform.parent.gameObject.GetComponent<RotationController>();
        movementController = transform.parent.parent.GetComponent<MovementController>();

        currentState = PlayerState.Idle;

        //Get all weapons that are children of the weapon container.
        //int i = 0;
        /*foreach(Transform child in weaponContainer.transform)
        {
            weaponObjects[i] = child.gameObject;
            i++;
        }*/

    }

    // Update is called once per frame
    void Update()
    {
        if (castTimer > 0)
        {
          castTimer -= Time.deltaTime;
        }
        if (telekinesisCastTimer > 0)
        {
          telekinesisCastTimer -= Time.deltaTime;
        }

        if (currentState == PlayerState.Idle)
        {
            comboResetTimer += Time.deltaTime;
            inRecovery = false;

            if(comboResetTimer > timeToComboReset)
            {
                lastSwingNum = 0;
            }

            //Time.timeScale = 1f;

            /*if (Input.GetMouseButtonDown(1)  || Input.GetButton("Fire2"))
            {
                FireTripleBlast();
            }
            if (castTimer > 0)
            {
              castTimer -= Time.deltaTime;
            }*/
        }

        if (currentState == PlayerState.Idle || inRecovery)
        {
            playerAnim.SetTrigger("Actionable");
        }
        else
        {
            playerAnim.ResetTrigger("Actionable");
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (lastSwingNum == 0)
            {
                //Debug.Log("Setting swing1");
                playerAnim.SetTrigger("Swing1");
                playerAnim.ResetTrigger("Dash");
                playerAnim.ResetTrigger("Swing2");
                playerAnim.ResetTrigger("Swing3");
            }
            else if((currentState == PlayerState.Swing1) || (currentState == PlayerState.Idle && comboResetTimer < timeToComboReset && lastSwingNum == 1))
            {
                //Debug.Log("Setting swing2");
                playerAnim.SetTrigger("Swing2");
                playerAnim.ResetTrigger("Dash");
                playerAnim.ResetTrigger("Swing1");
                playerAnim.ResetTrigger("Swing3");
            }
            else if ((currentState == PlayerState.Swing2) || (currentState == PlayerState.Idle && comboResetTimer < timeToComboReset && lastSwingNum == 2))
            {
                //Debug.Log("Setting swing3");
                playerAnim.SetTrigger("Swing3");
                playerAnim.ResetTrigger("Dash");
                playerAnim.ResetTrigger("Swing1");
                playerAnim.ResetTrigger("Swing2");
            }
            else if (currentState == PlayerState.Swing3)
            {
                //Debug.Log("Setting swing1");
                playerAnim.SetTrigger("Swing1");
                playerAnim.ResetTrigger("Dash");
                playerAnim.ResetTrigger("Swing2");
                playerAnim.ResetTrigger("Swing3");
            }
        }


        /*if ( Input.GetKeyDown(KeyCode.E) || Input.GetButton("Fire3") )
        {
            FireTripleBlast();
        }*/
        if ( Input.GetMouseButtonDown(1) ) // || Input.GetButton("Fire2")
        {
            Telekinesis();
        }
        /*else if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
        {
            hasClicked = true;
            playerAnim.SetBool("Swing Left", swingingL);
            playerAnim.SetTrigger("Swing");
            playerAnim.ResetTrigger("Thrust");
            playerAnim.ResetTrigger("Dash");
        }*/

       /* if (!thrustIsDashAttack)
        {
            if (hasClicked && (Input.GetMouseButton(0) || Input.GetButton("Fire1")))
            {
                holdTimer += Time.deltaTime;

                if (holdTimer >= thrustHoldTime * 10f) // Disabled right now as it doesn't deal damage
                {
                    hasClicked = false;
                    holdTimer = 0;
                    playerAnim.SetTrigger("Thrust");
                    playerAnim.ResetTrigger("Swing");
                    playerAnim.ResetTrigger("Dash");
                }
            }
            else
            {
                hasClicked = false;
                holdTimer = 0;
            }
        }
        else
        {
            if((Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1")) && inDash)
            {
                playerAnim.SetTrigger("Thrust");
                playerAnim.ResetTrigger("Swing");
                playerAnim.ResetTrigger("Dash");
            }
        }*/




    }

    public void StartAction(string name)
    {
        inRecovery = false;
        rotationController.snapToCurrentMouseAngle();

        playerAnim.ResetTrigger("Actionable");
        playerAnim.ResetTrigger("Swing1");
        playerAnim.ResetTrigger("Swing2");
        playerAnim.ResetTrigger("Swing3");
        playerAnim.ResetTrigger("Dash");
        
        if(name == "Idle")
        {
            currentState = PlayerState.Idle;
        }
        else if (name == "Dash")
        {
            movementController.startDash();
            inDash = true;
            currentState = PlayerState.Dash;
        }
        else if(name == "Swing1")
        {
            Debug.Log("Swing1");
            comboResetTimer = 0;
            lastSwingNum = 1;
            currentState = PlayerState.Swing1;
        }
        else if (name == "Swing2")
        {
            Debug.Log("Swing2");
            comboResetTimer = 0;
            lastSwingNum = 2;
            currentState = PlayerState.Swing2;
        }
        else if (name == "Swing3")
        {
            Debug.Log("Swing3");
            comboResetTimer = 0;
            lastSwingNum = 3;
            currentState = PlayerState.Swing3;
        }
    }

    private void initiateSwing()
    {
        playAttackSound();
    }


    public PlayerState GetActionState()
    {
        return currentState;
    }

    private void initiateThrust()
    {
        rotationController.snapToCurrentMouseAngle();
        rotForThrust = rotationController.GetRotationDirection();
        inThrust = true;
        inRecovery = false;
        playAttackSound();
        //playerAnim.ResetTrigger("Swing");
        //playerAnim.ResetTrigger("Thrust");
        playerAnim.ResetTrigger("Dash");
        playerAnim.ResetTrigger("Actionable");
    }

    private void initiateDash()
    {
        inDash = true;
        inRecovery = false;

        movementController.startDash();

        //playerAnim.ResetTrigger("Swing");
        //playerAnim.ResetTrigger("Thrust");
        playerAnim.ResetTrigger("Dash");
        playerAnim.ResetTrigger("Actionable");
        playerAnim.ResetTrigger("Swing1");
        playerAnim.ResetTrigger("Swing2");
        playerAnim.ResetTrigger("Swing3");
    }

    public void bufferDash()
    {
        //Debug.Log("Buffering dash");
        playerAnim.SetTrigger("Dash");
        //playerAnim.ResetTrigger("Thrust");
        playerAnim.ResetTrigger("Swing1");
        playerAnim.ResetTrigger("Swing2");
        playerAnim.ResetTrigger("Swing3");
    }

    private void addPushForward(float amount)
    {
        rotationController.snapToCurrentMouseAngle();
        rotForThrust = rotationController.GetRotationDirection();
        movementController.applyKnockback(transform.position - rotForThrust * 3, amount);
    }

    private void inRecoveryFrames()
    {
        inRecovery = true;
    }

    private void FireTripleBlast() {
        if (castTimer > 0) {
          return;
        } else {
          castTimer = waveSpellCooldown;
        }


        Vector3 waveSpellAnchor = transform.position + rotationController.GetRotationDirection();
        Vector3 curRotation = rotationController.GetRotationDirection();
        float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

        rotationController.snapToCurrentMouseAngle();

        Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle - waveSpellSpreadDegrees/2, 0) );
        Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle, 0) );
        Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle + waveSpellSpreadDegrees/2, 0) );


    }

    private void Telekinesis() {
      if (telekinesisCastTimer > 0 || inRecovery || rotationController.isFacingMouse == true) {
        return;
      } else {
        telekinesisCastTimer = telekinesisSpellCooldown;
      }

      Collider TyphisCollision = (movementController.gameObject).GetComponent<Collider>();

      Vector3 telekinesisSpellAnchor = transform.position + rotationController.GetRotationDirection() * 2f;

      Vector3 curRotation = rotationController.GetRotationDirection();
      float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

      GameObject telekinesis = Instantiate(telekinesisSpellPrefab, this.transform,  worldPositionStays:false );
      telekinesis.GetComponent<TelekinesisSpell>().TyphisCollider = TyphisCollision;
      telekinesis.GetComponent<TelekinesisSpell>().combatController = this; // This is delayed, so we have to wait on the other side.

      rotationController.isFacingMouse = true;
    }

    private void playAttackSound() {
      if(soundEffects != null)
        {
            float pitchMod = Random.Range(-0.25f, 0.25f);
            soundEffects.pitch = 1 + pitchMod;
            soundEffects.Play();
        }
    }

    public void objectWasThrown() {
      telekinesisCastTimer = telekinesisSpellCooldown;
      rotationController.isFacingMouse = false;
      Debug.Log("Telekinesis Done");
    }

    public bool isActionable()
    {
        return (isIdle || inRecovery);
    }
}
