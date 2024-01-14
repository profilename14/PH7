using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    Animator playerAnim;

    [SerializeField]
    bool inAnimation;

    public WeaponStats[] weaponScriptableObjects;

    public WeaponStats equippedWeapon;

    GameObject[] weaponObjects = new GameObject[3];

    [SerializeField]
    GameObject weaponContainer;

    [SerializeField]
    int equippedWeaponIndex;

    bool updateWeapon;

    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();

        //Get all weapons that are children of the weapon container.
        int i = 0;
        foreach(Transform child in weaponContainer.transform)
        {
            weaponObjects[i] = child.gameObject;
            i++;
        }

        equippedWeapon = weaponScriptableObjects[0];
    }

    // Update is called once per frame
    void Update()
    {
        if(!inAnimation)
        {
            if (Input.GetMouseButtonDown(0))
            {
                playerAnim.SetTrigger(equippedWeapon.weaponName);
                inAnimation = true;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
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
            }
        }
    }

    public void SetToIdle()
    {
        inAnimation = false;
    }
}
