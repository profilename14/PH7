using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class AoEProjectile : MyProjectile
{
    [SerializeField]
    Collider damageCollider;
    
    [SerializeField]
    ParticleSystem warningParticles;

    [SerializeField]
    float aoEDelay;

    [SerializeField]
    ParticleSystem aoEProjectiles;

    [SerializeField]
    float aoEDuration;

    public override void OnProjectileActivate()
    {
        StartCoroutine(DelayedAoE());
    }

    public IEnumerator DelayedAoE()
    {
        warningParticles.Play();

        yield return new WaitForSeconds(aoEDelay);

        damageCollider.enabled = true;

        aoEProjectiles.Play();

        yield return new WaitForSeconds(aoEDuration);

        damageCollider.enabled = false;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref damageCollider);
    }
#endif
}
