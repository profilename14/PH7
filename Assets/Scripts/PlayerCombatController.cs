using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    Animator playerAnim;

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
    int weaponSwingCombo;

    //Is the player in interruptible recovery frames of an attack animation?
    bool inRecovery;

    //Amount of time after the player enters idle where a new swing will not result in a combo.
    [SerializeField]
    float timeToResetCombo;

    public static bool playerIsIdle;

<<<<<<< Updated upstream
    [SerializeField] private float waveSpellSpreadDegrees;
    [SerializeField] private GameObject waveSpellPrefab;
=======
    [SerializeField]
    private bool comboResetCoroutineRunning;
>>>>>>> Stashed changes

    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();



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
        if(playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            playerIsIdle = true;
            inRecovery = false;

            if(!comboResetCoroutineRunning && canCombo)
            {
                StartCoroutine(WaitForResetCombo());
                Debug.Log("Wait for reset combo");
                comboResetCoroutineRunning = true;
            }

            if (Input.GetMouseButtonDown(0) && !canCombo)
            {
                weaponSwingCombo = 0;
                canCombo = false;
                playerAnim.SetTrigger(equippedWeapon.weaponName);
                comboResetCoroutineRunning = false;
                StartCoroutine(WaitForRecoveryFrames(equippedWeapon.t_combo0));
                StopCoroutine(WaitForResetCombo());
            }

            if (Input.GetMouseButtonDown(1))
            {
                FireTripleBlast();
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

        if((inRecovery || canCombo) && Input.GetMouseButtonDown(0))
        {
            canCombo = false;
            inRecovery = false;
            StopCoroutine(WaitForResetCombo());
            comboResetCoroutineRunning = false;

            if (weaponSwingCombo == 0)
            {
                playerAnim.SetTrigger("Combo");
                weaponSwingCombo = 1;
                playerAnim.SetInteger("Combo Number", 1);
                StartCoroutine(WaitForRecoveryFrames(equippedWeapon.t_combo1));
            }
            else if (weaponSwingCombo == 1)
            {
                playerAnim.SetTrigger("Combo");
                weaponSwingCombo = 2;
                playerAnim.SetInteger("Combo Number", 2);
                StartCoroutine(WaitForRecoveryFrames(equippedWeapon.t_combo2));
            }
            else if(weaponSwingCombo == 2)
            {
                weaponSwingCombo = 0;
                playerAnim.SetTrigger(equippedWeapon.weaponName);
                comboResetCoroutineRunning = false;
                StartCoroutine(WaitForRecoveryFrames(equippedWeapon.t_combo0));
            }
        }
    }

    public IEnumerator WaitForResetCombo()
    {
        yield return new WaitForSeconds(timeToResetCombo);
        canCombo = false;
        playerAnim.SetInteger("Combo Number", 0);
        weaponSwingCombo = 0;
        comboResetCoroutineRunning = false;
    }

    public IEnumerator WaitForRecoveryFrames(float framesToRecovery)
    {
        for (int i = 0; i < framesToRecovery; i++)
        {
            yield return null;
        }
        inRecovery = true;
        if(weaponSwingCombo != 2) canCombo = true;
        Debug.Log("Recovery frames");
    }

    private void FireTripleBlast() {


        RotationController rotationController = transform.parent.gameObject.GetComponent<RotationController>();
        Vector3 waveSpellAnchor = transform.position + rotationController.GetRotationDirection();
        Vector3 curRotation = rotationController.GetRotationDirection();
        float angle = -Mathf.Atan2(curRotation.z, curRotation.x) * Mathf.Rad2Deg + 90;

        Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle - waveSpellSpreadDegrees/2, 0) );
        Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle, 0) );
        Instantiate(waveSpellPrefab, waveSpellAnchor, Quaternion.Euler(0, angle + waveSpellSpreadDegrees/2, 0) );


    }
}
