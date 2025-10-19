using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class InstantPH : ColliderEffectField
{
    public float lifetime = 1f;

    private void Awake()
    {
        StartCoroutine(DestroySelf());
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(lifetime);

        Destroy(gameObject.GetComponent<SphereCollider>());
    }
}
