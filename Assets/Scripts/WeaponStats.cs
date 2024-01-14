using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponStats", order = 1)]
public class WeaponStats : ScriptableObject
{
    public string weaponName;
    public float damage;
    public float phDamage;
    public float knockback;
}
