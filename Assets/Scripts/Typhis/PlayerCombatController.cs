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
    public WeaponStats swordStats;

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


    public static bool inRecoveryPublic;

    [SerializeField] private float waveSpellSpreadDegrees;
    [SerializeField] private GameObject waveSpellPrefab;
    [SerializeField] private float waveSpellCooldown = 0.42f;
    private float castTimer = 0f;


    [SerializeField] private GameObject telekinesisSpellPrefab;
    [SerializeField] private float telekinesisSpellCooldown = 2.5f;
    [HideInInspector] public float telekinesisCastTimer = 0f;

    public bool isFacingMouse = false;

    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();
        rotationController = transform.parent.gameObject.GetComponent<RotationController>();
        movementController = transform.parent.parent.GetComponent<MovementController>();



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

        if (playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            isIdle = true;
            inRecovery = false;
            inThrust = false;
            inSwing = false;
            inDash = false;
            swingingL = false;
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
        else
        {
            isIdle = false;
        }

        if (isIdle || inRecovery)
        {
            playerAnim.SetTrigger("Actionable");
        }
        else
        {
            playerAnim.ResetTrigger("Actionable");
        }


        if ( Input.GetKeyDown(KeyCode.E) || Input.GetButton("Fire3") )
        {
            FireTripleBlast();
        }
        else if ( Input.GetMouseButtonDown(1) ) // || Input.GetButton("Fire2")
        {
            Telekinesis();
        }
        else if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
        {
            hasClicked = true;
            playerAnim.SetBool("Swing Left", swingingL);
            playerAnim.SetTrigger("Swing");
            playerAnim.ResetTrigger("Thrust");
            playerAnim.ResetTrigger("Dash");
        }

        if(hasClicked && (Input.GetMouseButton(0) || Input.GetButton("Fire1")))
        {
            holdTimer += Time.deltaTime;

            if(holdTimer >= thrustHoldTime)
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

    private void initiateSwing()
    {
        //Time.timeScale = 0.2f;
        inSwing = true;
        inRecovery = false;
        rotationController.snapToCurrentMouseAngle();
        playAttackSound();
        playerAnim.ResetTrigger("Swing");
        playerAnim.ResetTrigger("Thrust");
        playerAnim.ResetTrigger("Dash");
        playerAnim.ResetTrigger("Actionable");
    }

    private void initiateThrust()
    {
        rotationController.snapToCurrentMouseAngle();
        rotForThrust = rotationController.GetRotationDirection();
        inThrust = true;
        inRecovery = false;
        playAttackSound();
        playerAnim.ResetTrigger("Swing");
        playerAnim.ResetTrigger("Thrust");
        playerAnim.ResetTrigger("Dash");
        playerAnim.ResetTrigger("Actionable");
    }

    private void initiateDash()
    {
        inDash = true;
        inRecovery = false;

        playerAnim.ResetTrigger("Swing");
        playerAnim.ResetTrigger("Thrust");
        playerAnim.ResetTrigger("Dash");
        playerAnim.ResetTrigger("Actionable");


    }

    public void playDashAnim()
    {
      playerAnim.SetTrigger("Dash");
      playerAnim.ResetTrigger("Thrust");
      playerAnim.ResetTrigger("Swing");
    }

    private void addPushForward(float amount)
    {
        rotationController.snapToCurrentMouseAngle();
        rotForThrust = rotationController.GetRotationDirection();
        movementController.applyKnockback(transform.position - rotForThrust * 3, amount);
    }

    private void inRecoveryFrames()
    {
        swingingL = !swingingL;
        playerAnim.SetBool("Swing Left", swingingL);
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

      Vector3 telekinesisSpellAnchor = transform.position + rotationController.GetRotationDirection() * 2f;

      Vector3 curRotation = rotationController.GetRotationDirection();
      float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

      GameObject telekinesis = Instantiate(telekinesisSpellPrefab, this.transform,  worldPositionStays:false );
      telekinesis.GetComponent<TelekinesisSpell>().combatController = this; // This is delayed, so we have to wait on the other side.

      rotationController.isFacingMouse = true;
    }


    public bool isAttacking() {
      if (inSwing) {
        return true;
      }
      else {
        return false;
      }

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
}
