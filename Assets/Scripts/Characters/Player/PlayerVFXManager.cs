using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFXManager : CharacterVFXManager
{
    [SerializeField]
    private Renderer bodyRenderer;

    [SerializeField]
    private Material bodySwapMaterial;

    private Material bodyDefaultMaterial;

    [SerializeField]
    private Renderer[] swordRenderers;

    [SerializeField]
    private GameObject swordHitVFX;

    [SerializeField]
    private GameObject impactVFX;

    [SerializeField]
    private VisualEffect swing0VFX;

    [SerializeField]
    private VisualEffect swing1VFX;

    [SerializeField]
    private VisualEffect swing2VFX;

    [SerializeField]
    private VisualEffect swingDownVFX;

    [SerializeField]
    private ParticleSystem fullyChargedVFX;

    [SerializeField]
    private VisualEffect chargedSwingVFX;

    [SerializeField]
    Color dashColor;

    [SerializeField]
    Color damageFlashColor;

    [SerializeField]
    float damageFlashTime;
    
    [SerializeField]
    Color algaeGlowColor;

    [SerializeField]
    float algaeGlowIntensity;

    [SerializeField]
    Color swordGlowColor;

    [SerializeField]
    float swordGlowIntensity;

    protected override void Awake()
    {
        base.Awake();
        bodyDefaultMaterial = bodyRenderer.material;
        SetDefaultEmissionColors(swordRenderers);
    }

    public override void DeathVFX()
    {
        Debug.Log("You died!");
    }

    public override void TookDamageVFX(Vector3 collisionPoint, Vector3 sourcePos)
    {
        ResetEmissionColors(swordRenderers);
        StartCoroutine(FlashEmissionColor(damageFlashTime, damageFlashColor, baseRenderers));
        StartCoroutine(FlashBodyColor(damageFlashTime, damageFlashColor));
        //Instantiate(bloodParticles, collisionPoint + Vector3.up, Quaternion.identity).transform.up = collisionPoint - sourcePos;
    }

    public void SwordSwingVFX(SwordSwingType vfx)
    {
        switch (vfx)
        {
            case SwordSwingType.Swing0:
                swing0VFX.Play();
                break;
            case SwordSwingType.Swing1:
                swing1VFX.Play();
                break;
            case SwordSwingType.Swing2:
                swing2VFX.Play();
                break;
            case SwordSwingType.SwingDown:
                swingDownVFX.Play();
                break;
            case SwordSwingType.ChargedSwing:
                chargedSwingVFX.Play();
                break;
            default:
                break;
        }
    }

    public void SwordHitVFX(Vector3 hitPosition)
    {
        Instantiate(swordHitVFX, hitPosition + Vector3.up, Quaternion.identity);
        Instantiate(impactVFX, hitPosition + Vector3.up, Quaternion.identity);
    }

    public void FullyChargedVFX()
    {
        fullyChargedVFX.Play();
        SetEmissionColor(baseRenderers, algaeGlowColor * algaeGlowIntensity);
        SetEmissionColor(swordRenderers, swordGlowColor * swordGlowIntensity);
        SetBodyEmissionColor(algaeGlowColor * algaeGlowIntensity);
    }

    public void EndChargeVFX()
    {
        ResetEmissionColors(baseRenderers);
        ResetEmissionColors(swordRenderers);
        ResetBodyEmissionColor();
    }

    public IEnumerator FlashEmissionColor(float seconds, Color color, Renderer[] renderers)
    {
        SetEmissionColor(renderers, color);

        yield return new WaitForSeconds(seconds);

        ResetEmissionColors(baseRenderers);
    }

    public IEnumerator FlashBodyColor(float seconds, Color color)
    {
        SetBodyEmissionColor(color);

        yield return new WaitForSeconds(seconds);

        ResetBodyEmissionColor();
    }

    public void SetBodyEmissionColor(Color color)
    {
        bodyRenderer.material = bodySwapMaterial;
        bodyRenderer.material.SetColor("_EmissionColor", color);
    }

    public void ResetBodyEmissionColor()
    {
        bodyRenderer.material = bodyDefaultMaterial;
    }
}