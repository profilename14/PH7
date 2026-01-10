using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SlashAttackVFXManager : EnemyVFXManager
{
    [SerializeField]
    VisualEffect slashVFX;

    public void PlaySlashVFX()
    {
        slashVFX.Play();
    }
}
