using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class ColliderEffectField : MonoBehaviour
{
    public Chemical effectType;

    public float damageOnEnter;

    public bool enableDamageOverTime;

    public float damageOverTime;

    public float damageTickInterval;

    public Vector2 staticKnockback;

    public float dynamicKnockback;

    public bool causeHit;

    public List<IHittable> doTEntities = new();

    private void OnDisable()
    {
        doTEntities.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Hit something");
        if (!gameObject.activeInHierarchy) return;

        // If this script is disabled, then the effect field is disabled
        if (this.enabled == false) return;

        // Check if we have collided with a hittable object.
        IHittable hittableScript = other.gameObject.GetComponentInParentOrChildren<IHittable>();
        if (hittableScript == null) return;

        //Debug.Log("Hittable: " + other.gameObject.name);

        hittableScript.Hit(this, damageOnEnter);

        if (enableDamageOverTime)
        {
            doTEntities.Add(hittableScript);
            StartCoroutine(DamageOverTimeTicks());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enableDamageOverTime) return;

        //Debug.Log("Hit something");
        if (!gameObject.activeInHierarchy) return;

        // If this script is disabled, then the effect field is disabled
        if (this.enabled == false) return;

        // Check if we have collided with a hittable object.
        IHittable hittableScript = other.gameObject.GetComponentInParentOrChildren<IHittable>();
        if (hittableScript == null) return;

        if (hittableScript is Enemy) return;

        //Debug.Log("Hittable: " + other.gameObject.name);

        foreach(IHittable h in doTEntities)
        {
            if(h == hittableScript)
            {
                doTEntities.Remove(hittableScript);
                return;
            }
        }
    }

    public IEnumerator DamageOverTimeTicks()
    {
        yield return new WaitForSeconds(damageTickInterval);

        if (doTEntities.Count == 0) StopAllCoroutines();

        foreach (IHittable h in doTEntities)
        {
            h.Hit(this, damageOverTime);
        }

        StartCoroutine(DamageOverTimeTicks());
    }
}
