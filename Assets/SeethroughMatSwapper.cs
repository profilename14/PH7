using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeethroughMatSwapper : MonoBehaviour
{
    public Material defaultMaterial;
    public Material seethroughMaterial;

    public bool isSeethrough;

    public void ResetMaterial()
    {
        isSeethrough = false;
        GetComponent<MeshRenderer>().material = defaultMaterial;
    }

    public void MakeSeethrough()
    {
        isSeethrough = true;
        GetComponent<MeshRenderer>().material = seethroughMaterial;
    }
}
