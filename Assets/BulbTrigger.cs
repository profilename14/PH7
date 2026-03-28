using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BulbTrigger : MonoBehaviour, IHittable
{
    [SerializeField] GameObject containerObject;
    [SerializeField] ParticleSystem breakEffect;
    [SerializeField] Collider coll;

    bool isBroken = false;

    [SerializeField]
    UnityEvent onBreak;

    [SerializeField]
    float regrowTime = 10f;

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        if (isBroken) return;

        breakEffect.Play();            

        onBreak.Invoke();

        containerObject.SetActive(false);
        coll.enabled = false;
        isBroken = true;
        StartCoroutine(RegrowBulb());
    }

    public void Hit(MyProjectile projectile, Vector3 hitPoint)
    {
        return;
    }

    public void Hit(ColliderEffectField colliderEffectField, float damage)
    {
        return;
    }

    IEnumerator RegrowBulb()
    {
        yield return new WaitForSeconds(regrowTime);

        containerObject.SetActive(true);
        coll.enabled = true;
        isBroken = false;
    }
}
