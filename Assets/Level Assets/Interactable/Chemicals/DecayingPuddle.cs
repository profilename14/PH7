using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecayingPuddle : MonoBehaviour
{
    public float decaySpeed;
    [SerializeField] GameObject parent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        parent.transform.position -= Vector3.up * decaySpeed * Time.deltaTime;
    }
}
