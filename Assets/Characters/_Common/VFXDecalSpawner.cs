using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXDecalSpawner : MonoBehaviour
{
    public GameObject decal;

    public void SpawnDecal()
    {
        Instantiate(decal, this.transform.position, this.transform.rotation);
    }
}
