using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFXManager : CharacterVFXManager
{
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
    private VisualEffect chargedSwingVFX;

    public override void DeathVFX()
    {
        Debug.Log("You died!");
    }

    public override void TookDamageVFX(Vector3 collisionPoint, Vector3 source)
    {
        Debug.Log("Took damage!");
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
}
