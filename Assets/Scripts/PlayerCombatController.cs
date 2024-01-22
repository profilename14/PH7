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

    //Amount of time after the player initiates an attack when they can start the next swing in the combo.
    [SerializeField]
    float timeToCombo;

    //Amount of time after the player initiates an attack where a new swing will not result in a combo.
    [SerializeField]
    float timeToResetCombo;

    public static bool playerIsIdle;

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

            if (Input.GetMouseButtonDown(0))
            {
                playerAnim.SetTrigger(equippedWeapon.weaponName);
                StartCoroutine(WaitForCombo());
                StopCoroutine(WaitForResetCombo());
                StartCoroutine(WaitForResetCombo());
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

        if(canCombo && Input.GetMouseButtonDown(0))
        {
            if (weaponSwingCombo == 1)
            {
                playerAnim.SetTrigger("Combo");
                canCombo = false;
                weaponSwingCombo = 0;
                StopAllCoroutines();
            }
        }
    }

    public IEnumerator WaitForCombo()
    {
        yield return new WaitForSeconds(timeToCombo);
        canCombo = true;
        weaponSwingCombo++;
    }

    public IEnumerator WaitForResetCombo()
    {
        yield return new WaitForSeconds(timeToResetCombo);
        canCombo = false;
        weaponSwingCombo = 0;
    }
}
