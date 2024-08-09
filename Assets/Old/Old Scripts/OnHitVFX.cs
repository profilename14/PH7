using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitVFX : MonoBehaviour
{
    public GameObject hitVFX;

    public void HitVFX()
    {
        Instantiate(hitVFX, transform.position, Quaternion.identity);
    }
}
