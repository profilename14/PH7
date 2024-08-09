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

    [Header("Frames Until Recovery Frames")]
    public float t_combo0;
    public float t_combo1;
    public float t_combo2;
}
