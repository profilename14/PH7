using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AttackData : ScriptableObject
{
    public float damage;
    public float knockback;
    public Chemical type;
}
