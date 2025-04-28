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
    private GameObject landVFX;
    
    [SerializeField]
    private GameObject jumpVFX;

    [SerializeField]
    private ParticleSystem runVFX;
    private ParticleSystem instantiatedRunVFX;


    [SerializeField]
    Color dashColor;

    [SerializeField]
    private float dashGlowIntensity;

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

    [SerializeField]
    float hitEffectIntensity = 3; // lower is stronger
    [SerializeField]
    Material hitEffect;

    protected override void Awake()
    {
        base.Awake();
        bodyDefaultMaterial = bodyRenderer.material;
        SetDefaultEmissionColors(swordRenderers);
        
        hitEffect.SetFloat("_VignettePower", 30);
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
        StartCoroutine(FlashHitVignette(damageFlashTime * 3, hitEffectIntensity));
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
            case SwordSwingType.DashSwing:
                swing0VFX.Play();
                break;
            default:
                break;
        }
    }

    public void PauseSwordSwingVFX(SwordSwingType vfx, float pauseTime)
    {
        switch (vfx)
        {
            case SwordSwingType.Swing0:
                StartCoroutine(SwordSwingPause(swing0VFX, pauseTime));
                break;
            case SwordSwingType.Swing1:
                StartCoroutine(SwordSwingPause(swing1VFX, pauseTime));
                break;
            case SwordSwingType.Swing2:
                StartCoroutine(SwordSwingPause(swing2VFX, pauseTime));
                break;
            case SwordSwingType.SwingDown:
                StartCoroutine(SwordSwingPause(swingDownVFX, pauseTime));
                break;
            case SwordSwingType.ChargedSwing:
                StartCoroutine(SwordSwingPause(chargedSwingVFX, pauseTime));
                break;
            case SwordSwingType.DashSwing:
                StartCoroutine(SwordSwingPause(swing0VFX, pauseTime));
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

    public void StartDashVFX()
    {
        SetEmissionColor(baseRenderers, dashColor * dashGlowIntensity);
        SetBodyEmissionColor(dashColor * dashGlowIntensity);
    }

    public void EndDashVFX()
    {
        ResetEmissionColors(baseRenderers);
        ResetBodyEmissionColor();
    }

    public void StartLandVFX(Vector3 landPosition)
    {
        Instantiate(landVFX, landPosition, Quaternion.Euler(-90, 0, 0));
    }

    public void StartJumpVFX(Vector3 jumpPosition)
    {
        Instantiate(jumpVFX, jumpPosition, Quaternion.Euler(-90, 0, 0));
    }

    public void StartRunVFX(Transform transform)
    {
        if (!instantiatedRunVFX)
        {
            instantiatedRunVFX = Instantiate(runVFX, transform);
        }

        instantiatedRunVFX.Play();
        
    }

    public void StopRunVFX()
    {
        instantiatedRunVFX.Stop();
    }

    public IEnumerator SwordSwingPause(VisualEffect vfx, float seconds)
    {
        vfx.pause = true;

        yield return new WaitForSeconds(seconds);

        vfx.pause = false;
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

    public IEnumerator FlashHitVignette(float seconds, float intensity)
    {
        float elapsedSeconds = 0f;

        while (elapsedSeconds < seconds)
        {
            elapsedSeconds += Time.deltaTime;
            hitEffect.SetFloat("_VignettePower", Mathf.Lerp(intensity, 7.5f, elapsedSeconds/seconds));
            yield return null;
        }

        
            hitEffect.SetFloat("_VignettePower", Mathf.Lerp(intensity, 30, elapsedSeconds/seconds));
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
