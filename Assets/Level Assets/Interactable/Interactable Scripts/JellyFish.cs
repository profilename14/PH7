using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JellyFish : MonoBehaviour
{
  Rigidbody bubbleRigidbody;
    public Vector3 direction = new Vector3(0, 0, 0);
    public float force;

    [SerializeField] float lifespan = 10f;
    float lifespanTimer = 0;



    void Start() {
      bubbleRigidbody = GetComponent<Rigidbody>();

      bubbleRigidbody.AddForce((direction).normalized * force, ForceMode.Impulse);

      lifespanTimer = lifespan;
    }

    void Update()
    {
      bubbleRigidbody.velocity *= 0.98f;

      lifespanTimer -= Time.deltaTime;
      if (lifespanTimer <= 0)
      {
        Destroy(gameObject);
      }
    }




}
