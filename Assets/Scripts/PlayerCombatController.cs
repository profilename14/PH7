using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    Animator playerAnim;
    RotationController rotationController;
    PlayerStats playerStats;
    [SerializeField] AudioSource soundEffects;

    [Header("STATS")]
    public float health;
    public int ph;

    [Header("ATTACKS")]
    public WeaponStats[] weaponScriptableObjects;

    public WeaponStats equippedWeapon;

    GameObject[] weaponObjects = new GameObject[3];

    [SerializeField]
    GameObject weaponContainer;

    [SerializeField]
    int equippedWeaponIndex;

    [SerializeField]
    bool inAnimation;

    [SerializeField]
    bool canCombo;

    bool updateWeapon;

    [SerializeField]
    public int weaponSwingCombo;

    //Is the player in interruptible recovery frames of an attack animation?
    [SerializeField]
    public bool inRecovery;

    //Amount of time after the player enters idle where a new swing will not result in a combo.
    [SerializeField]
    float timeToResetCombo;

    public static bool playerIsIdle;
    public static bool inRecoveryPublic;

    [SerializeField] private float waveSpellSpreadDegrees;
    [SerializeField] private GameObject waveSpellPrefab;
    [SerializeField] private float waveSpellCooldown = 0.75f;
    [SerializeField] private float waveSpellCost = 2f;
    private float waveCastTimer = 0f;

    [SerializeField] private GameObject rainSpellPrefab;
    [SerializeField] private float rainSpellCooldown = 10f;
    private float rainCastTimer = 0f;

    [SerializeField] private GameObject hydroxideSpellPrefab;
    [SerializeField] private float hydroxideSpellCooldown = 5f;
    private float hydroxideCastTimer = 0f;


    [SerializeField]
    private bool comboResetCoroutineRunning;

    [SerializeField]
    private bool recoveryCoroutineRunning;

    private float comboResetTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();
        rotationController = transform.parent.gameObject.GetComponent<RotationController>();
        playerStats = transform.parent.parent.gameObject.GetComponent<PlayerStats>();



        //Get all weapons that are children of the weapon container.
        //int i = 0;
        /*foreach(Transform child in weaponContainer.transform)
        {
            weaponObjects[i] = child.gameObject;
            i++;
        }*/

        equippedWeapon = weaponScriptableObjects[0];
    }

    // Update is called once per frame
    void Update()
    {
        inRecoveryPublic = inRecovery;
        if(playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            playerIsIdle = true;

            canCombo = false;
            inRecovery = false;

            /*if (!canCombo) {
              inRecovery = false;
            }
             if (canCombo && comboResetTimer <= 0) {
               comboResetTimer = timeToResetCombo;
             }

            if (comboResetTimer > 0) {
              comboResetTimer -= Time.deltaTime;
              if (comboResetTimer <= 0) {
                canCombo = false;
              }
            }/*


            /*if(!comboResetCoroutineRunning && canCombo && inRecovery)
            {
                //StartCoroutine(WaitForResetCombo());
                //Debug.Log("Wait for reset combo");
                comboResetCoroutineRunning = true;
            }*/

            if (Input.GetMouseButtonDown(0) && !canCombo)
            {
                if (recoveryCoroutineRunning == true) {
                  // Not doing this right here was what was causing bugs.
                  return;
                }
                rotationController.snapToCurrentAngle();
                weaponSwingCombo = 0;
                canCombo = false;
                playerAnim.SetTrigger(equippedWeapon.weaponName);
                //comboResetCoroutineRunning = false;
                StartCoroutine(WaitForRecoveryFrames(equippedWeapon.t_combo0));
                //StopCoroutine(WaitForResetCombo());
                comboResetTimer = 0;
                playAttackSound();
            }

            if (Input.GetMouseButtonDown(1))
            {
                FireTripleBlast();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                SummonRain();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                HydroxideDrain();
            }
            if (waveCastTimer > -waveSpellCooldown) // waveCastTimer goes negative for healing phase
            {
              waveCastTimer -= Time.deltaTime;
              playerStats.ph += Time.deltaTime * waveSpellCost * 0.5f;
              // Fully recover your PH, but only if you fire at half speed
              // Optionally lower your pH each step for dramatically better fire rate
            }
            if (rainCastTimer > 0)
            {
              rainCastTimer -= Time.deltaTime;
              if (playerStats.strongBaseMode) { // lower cooldowns in strong base mode
                rainCastTimer -= Time.deltaTime / 2;
              }
            }
            if (hydroxideCastTimer > 0)
            {
              hydroxideCastTimer -= Time.deltaTime;
              if (playerStats.strongBaseMode) { // Chain hydroxides especially to keep SBM going
                hydroxideCastTimer -= Time.deltaTime;
              }
            }

            //For now, weapon switching is disabled until we implement the other weapons.

            /*if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (equippedWeaponIndex != 0) updateWeapon = true;
                equippedWeaponIndex = 0;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (equippedWeaponIndex != 1) updateWeapon = true;
                equippedWeaponIndex = 1;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (equippedWeaponIndex != 2) updateWeapon = true;
                equippedWeaponIndex = 2;
            }

            //Updates the equipped weapon if it was changed
            if (updateWeapon)
            {
                equippedWeapon = weaponScriptableObjects[equippedWeaponIndex];
                for (int i = 0; i < weaponObjects.Length; i++)
                {
                    if (equippedWeaponIndex == i) weaponObjects[i].SetActive(true);
                    else weaponObjects[i].SetActive(false);
                }

                updateWeapon = false;
            }*/
        }
        else
        {
            playerIsIdle = false;
        }

        if((inRecovery && canCombo) && Input.GetMouseButtonDown(0))
        {
            if (recoveryCoroutineRunning == true) {
              Debug.Log("WARNING: recovery Coroutine running yet inRecovery");
              return;
            }
            canCombo = false;
            inRecovery = false;
            //StopCoroutine(WaitForResetCombo());
            comboResetCoroutineRunning = false;
            comboResetTimer = 0;



            if (weaponSwingCombo == 0)
            {
                playAttackSound();
                playerAnim.SetTrigger("Combo");
                weaponSwingCombo = 1;
                playerAnim.SetInteger("Combo Number", 1);

                StartCoroutine(WaitForRecoveryFrames(equippedWeapon.t_combo1));
                //Debug.Log(equippedWeapon.t_combo1);
                recoveryCoroutineRunning = true;
            }
            else if (weaponSwingCombo == 1)
            {
                playAttackSound();
                playerAnim.SetTrigger("Combo");
                weaponSwingCombo = 2;
                playerAnim.SetInteger("Combo Number", 2);
                StartCoroutine(WaitForRecoveryFrames(equippedWeapon.t_combo2));
                //Debug.Log(equippedWeapon.t_combo2);
                recoveryCoroutineRunning = true;
            }
            else if(weaponSwingCombo == 2)
            {
                playAttackSound();
                weaponSwingCombo = 0;
                playerAnim.SetTrigger(equippedWeapon.weaponName);
                playerAnim.SetInteger("Combo Number", 0);
                comboResetCoroutineRunning = false;
                StartCoroutine(WaitForRecoveryFrames(equippedWeapon.t_combo0));
                //Debug.Log(equippedWeapon.t_combo0);
                recoveryCoroutineRunning = true;
            }
            rotationController.snapToCurrentAngle();
        }
    }

    public IEnumerator WaitForResetCombo()
    {
        /*
        yield return new WaitForSeconds(timeToResetCombo);
        while (!playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
          yield return new WaitForSeconds(0.1f);
          Debug.Log("Stuck waiting for idle to combo reset");
        }

          canCombo = false;
          playerAnim.SetInteger("Combo Number", 0);
          weaponSwingCombo = 0;
          comboResetCoroutineRunning = false;

        */
        // We can use this later, but its 2am so I'm just gonna make a timer at this point.
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator WaitForRecoveryFrames(float framesToRecovery)
    {
        inRecovery = false;
        recoveryCoroutineRunning = true;
        float timeToWait = framesToRecovery / 60.0f;
        float attackSpeedTimeMult = 1;
        if (playerStats.strongBaseMode) { // Prob a better way to do this
          attackSpeedTimeMult *= 0.8f;
        }
        if (playerStats.rainPower) {
          if (5.5f < playerStats.ph && playerStats.ph < 8.5f) {
            attackSpeedTimeMult *= 0.8f; // 0.64 combined
          } else {
            attackSpeedTimeMult *= 0.9f;
          }
        }
        timeToWait *= attackSpeedTimeMult;

        while (timeToWait > 0)
        {
            timeToWait -= Time.deltaTime;
            //Debug.Log(timeToWait);
            yield return null;
        }
        // Don't think attacks should be fps dependent: Causes attacks to become sluggish randomly

        recoveryCoroutineRunning = false;
        inRecovery = true;

        if(weaponSwingCombo != 2) canCombo = true;
        //Debug.Log("Recovery frames");

        //StartCoroutine(WaitForResetCombo());
        //comboResetCoroutineRunning = true;
    }

    private void FireTripleBlast() {
        if (waveCastTimer > 0 || recoveryCoroutineRunning) {
          return;
        } else {
          waveCastTimer = waveSpellCooldown;
        }

        playerStats.ph -= waveSpellCost;

        Vector3 waveSpellAnchor = transform.position + rotationController.GetRotationDirection();
        Vector3 curRotation = rotationController.GetRotationDirection();
        float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

        Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle - waveSpellSpreadDegrees/2, 0) );
        Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle, 0) );
        Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle + waveSpellSpreadDegrees/2, 0) );
        if (playerStats.strongBaseMode) {
          Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle - waveSpellSpreadDegrees, 0) );
          Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle + waveSpellSpreadDegrees, 0) );
        }

    }

    private void SummonRain() {
      if (rainCastTimer > 0 || recoveryCoroutineRunning) {
        return;
      } else {
        rainCastTimer = rainSpellCooldown;
      }

      Vector3 rainSpellAnchor = transform.position + rotationController.GetRotationDirection() * 7.5f;

      GameObject rain = Instantiate(rainSpellPrefab, rainSpellAnchor, Quaternion.Euler(0, 0, 0) );

      // Has to be linked to change attack speed and its damage at different pHs
      rain.GetComponent<RainSpell>().playerStats = playerStats;
    }

    private void HydroxideDrain() {
      if (hydroxideCastTimer > 0 || recoveryCoroutineRunning) {
        return;
      } else {
        hydroxideCastTimer = hydroxideSpellCooldown;
      }

      Vector3 hydroxideSpellAnchor = transform.position + rotationController.GetRotationDirection() * 2f;

      Vector3 curRotation = rotationController.GetRotationDirection();
      float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

      GameObject hydroxide = Instantiate(hydroxideSpellPrefab, this.transform,  worldPositionStays:false );
      hydroxide.GetComponent<HydroxideSpell>().playerStats = playerStats;
    }

    public bool isAttacking() {
      if (comboResetTimer > timeToResetCombo * 0.75 || recoveryCoroutineRunning) {
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
}
