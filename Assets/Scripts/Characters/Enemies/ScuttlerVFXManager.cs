using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ScuttlerVFXManager : EnemyVFXManager
{
    [SerializeField]
    VisualEffect clawVFX;

    public void PlayClawVFX()
    {
        clawVFX.Play();
    }
}
