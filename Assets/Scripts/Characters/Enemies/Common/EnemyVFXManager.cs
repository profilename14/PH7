using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVFXManager : CharacterVFXManager
{
    [SerializeField]
    private Color damageFlashColor;

    [SerializeField]
    private float damageFlashIntensity = 1;

    [SerializeField]
    private float damageFlashTime = 0.2f;

    [SerializeField]
    private GameObject bloodParticles;

    public override void DeathVFX()
    {
        //Debug.Log("Enemy died!");
    }

    public override void TookDamageVFX(Vector3 collisionPoint, Vector3 sourcePos)
    {
        StartCoroutine(FlashEmissionColor(damageFlashTime, damageFlashColor * damageFlashIntensity));
        Instantiate(bloodParticles, collisionPoint + Vector3.up, Quaternion.identity).transform.up = collisionPoint - sourcePos;
    }

    public IEnumerator FlashEmissionColor(float seconds, Color color)
    {
        SetEmissionColor(baseRenderers, color);

        yield return new WaitForSeconds(seconds);

        Debug.Log("Resetting emission colors");

        ResetEmissionColors(baseRenderers);
    }
}
