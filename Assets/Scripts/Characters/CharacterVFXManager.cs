using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public abstract class CharacterVFXManager : MonoBehaviour
{
    [SerializeField]
    protected Renderer[] renderers;

    protected List<Color> defaultEmissionColors = new();
    protected bool setDefaultEmissionColors = true;

    protected void SetEmissionColor(Color c)
    {
        foreach (Renderer m in renderers)
        {
            foreach (Material mat in m.materials)
            {
                mat.EnableKeyword("_EMISSION");
                if (setDefaultEmissionColors) defaultEmissionColors.Add(mat.GetColor("_EmissionColor"));
                mat.SetColor("_EmissionColor", c);
            }
        }
    }

    protected void ResetEmissionColors()
    {
        int i = 0;

        foreach (Renderer m in renderers)
        {
            foreach (Material mat in m.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", defaultEmissionColors[i]);
                i++;
            }
        }
    }

    public abstract void TookDamageVFX(Vector3 collisionPoint, Vector3 source);

    public abstract void DeathVFX();
}
