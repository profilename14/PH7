using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    
    [SerializeField]
    private int _HealthMax;
    public int healthMax => _HealthMax;

    private int _ArmorMax = 5;
    public int armorMax => _ArmorMax;

    private int _CurrentArmor = 0;
    public int currentArmor => _CurrentArmor;

    private double _AcidResource = 0;
    public double acid => _AcidResource;
    
    [SerializeField]
    public double alkaline = 0;

    [SerializeField]
    public int lowHealth = 1;

    PlayerVFXManager vfxManager;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ModifyAlkaline(double alkaline)
    {
        
        this.alkaline += alkaline;
        if (this.alkaline > 10)
        {
            this.alkaline = 10;
        }
        /*else if (alkaline + _AcidResource > 14)
        {
            _AcidResource = 14 - alkaline;
        }*/
    }

    public void ModifyAcid(double acid)
    {
        return;
        /*_AcidResource += acid;
        if (_AcidResource > 14)
        {
            _AcidResource = 14;
        }
        else if (_AcidResource + alkaline > 14)
        {
            alkaline = 14 - _AcidResource;
        }*/
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        Player.instance.cinemachineManager.ScreenShake(2f, 2f);
    }

    public override void SetHealth(float newHealth)
    {
        base.SetHealth(newHealth);

        if (vfxManager == null) vfxManager = (PlayerVFXManager)Player.instance.VFXManager;

        if (health <= lowHealth) vfxManager.SetIsLowHealth(true);
        else vfxManager.SetIsLowHealth(false);
    }
}
