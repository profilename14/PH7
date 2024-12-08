using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StriderVFXManager : EnemyVFXManager
{
    [SerializeField]
    TrailRenderer dashTrail;

    [SerializeField]
    Color dashColor;

    [SerializeField]
    float dashColorIntensity = 1;

    public void SetIsDashGlowing(bool isGlowing)
    {
        if(isGlowing)
        {
            SetEmissionColor(baseRenderers, dashColor * dashColorIntensity);
        }
        else
        {
            ResetEmissionColors(baseRenderers);
        }
    }

    public void SetDashTrailEmission(bool isEmitting)
    {
        dashTrail.emitting = isEmitting;
    }
}
