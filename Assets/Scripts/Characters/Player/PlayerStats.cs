using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    
    [SerializeField]
    private int _HealthMax = 10;
    public int healthMax => _HealthMax;

    private int _ArmorMax = 5;
    public int armorMax => _ArmorMax;

    private int _CurrentArmor = 0;
    public int currentArmor => _CurrentArmor;

    private double _AcidResource = 0;
    public double acid => _AcidResource;
    
    private double _AlkalineResource = 0;
    public double alkaline => _AlkalineResource;

    public void ModifyAlkaline(double alkaline)
    {
        _AlkalineResource += alkaline;
    }

    public void ModifyAcid(double acid)
    {
        _AcidResource += acid;
    }
}
