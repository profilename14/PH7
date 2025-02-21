using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FloatingBubble : MonoBehaviour
{
  Rigidbody bubbleRigidbody;
    public Vector3 direction = new Vector3(0, 1, 0);
    public float force;

    [SerializeField] float lifespan = 10f;
    float lifespanTimer = 0;

    [SerializeField] float slowdownRate = 0.9f;



    void Start() {
      bubbleRigidbody = GetComponent<Rigidbody>();

      bubbleRigidbody.AddForce((direction).normalized * force, ForceMode.Impulse);

      lifespanTimer = lifespan;
    }

    void Update()
    {
      lifespanTimer -= Time.deltaTime;
      if (lifespanTimer <= 0)
      {
        Destroy(gameObject);
      }
    }

    private void FixedUpdate()
    {
        bubbleRigidbody.velocity *= slowdownRate;
    }




}
