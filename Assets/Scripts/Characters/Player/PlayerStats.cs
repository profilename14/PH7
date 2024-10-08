using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    
    [SerializeField]
    private float _HealthMax = 10;
    public float healthMax => _HealthMax;
    
    [SerializeField]
    private float _ArmorMax = 5;
    public float armorMax => _ArmorMax;
}
