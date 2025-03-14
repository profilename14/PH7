using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class copyPosition : MonoBehaviour
{
    [SerializeField] Transform transformToCopy;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = transformToCopy.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position =transformToCopy.position;
    }
}
