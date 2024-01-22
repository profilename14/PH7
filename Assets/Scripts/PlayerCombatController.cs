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

    //Amount of time after the player initiates an attack when they can start the next swing in the combo. Going to the idle state resets your combo.
    [SerializeField]
    float timeToCombo;

    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();

        //Get all weapons that are children of the weapon container.
        int i = 0;
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
            canCombo = false;
            weaponSwingCombo = 0;

            if (Input.GetMouseButtonDown(0))
            {
                playerAnim.SetTrigger(equippedWeapon.weaponName);
                StartCoroutine(WaitForCombo());
            }

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
            if(Input.GetMouseButtonDown(0) && weaponSwingCombo == 1 && canCombo)
            {
                playerAnim.SetTrigger("Combo");
                weaponSwingCombo = 2;
            }
        }
    }

    public IEnumerator WaitForCombo()
    {
        yield return new WaitForSeconds(timeToCombo);
        canCombo = true;
        weaponSwingCombo++;
    }
}
