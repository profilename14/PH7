using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FloatingBubble : MonoBehaviour
{
  Rigidbody bubbleRigidbody;
    public Vector3 direction = new Vector3(0, 1, 0);
    public float force;



    void Start() {
      bubbleRigidbody = GetComponent<Rigidbody>();

      bubbleRigidbody.AddForce((direction).normalized * force, ForceMode.Impulse);
    }

    void Update()
    {
      bubbleRigidbody.velocity *= 0.98f;
    }




}
