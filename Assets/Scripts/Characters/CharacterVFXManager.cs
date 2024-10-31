using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public abstract class CharacterVFXManager : MonoBehaviour
{
    [SerializeField]
    protected Renderer[] baseRenderers;

    protected Dictionary<Renderer[], List<Color>> defaultEmissionColors = new();

    protected virtual void Awake()
    {
        SetDefaultEmissionColors(baseRenderers);
    }

    protected void SetDefaultEmissionColors(Renderer[] renderers)
    {
        defaultEmissionColors.Add(renderers, new List<Color>());
        foreach (Renderer m in renderers)
        {
            foreach (Material mat in m.materials)
            {
                defaultEmissionColors[renderers].Add(mat.GetColor("_EmissionColor"));
            }
        }
    }

    protected void SetEmissionColor(Renderer[] renderers, Color c)
    {
        foreach (Renderer m in renderers)
        {
            foreach (Material mat in m.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", c);
            }
        }
    }

    protected void ResetEmissionColors(Renderer[] renderers)
    {
        int i = 0;

        foreach (Renderer m in renderers)
        {
            foreach (Material mat in m.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", defaultEmissionColors[renderers][i]);
                i++;
            }
        }
    }

    public abstract void TookDamageVFX(Vector3 collisionPoint, Vector3 source);

    public abstract void DeathVFX();
}
