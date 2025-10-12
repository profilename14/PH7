using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class FloatingBubble : MonoBehaviour
{
  Rigidbody bubbleRigidbody;
    public Vector3 direction = new Vector3(0, 1, 0);
    public float force;

    [SerializeField] float lifespan = 10f;
    float lifespanTimer = 0;

    [SerializeField] float slowdownRate = 0.9f;

    [SerializeField] UnityEvent onPop;

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
            onPop.Invoke();
            gameObject.SetActive(false);
      }
    }

    private void FixedUpdate()
    {
        if(bubbleRigidbody.velocity.magnitude > 0) bubbleRigidbody.velocity *= slowdownRate;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10 || /*other.gameObject.layer == 17 ||*/ other.gameObject.layer == 18)
        {
            onPop.Invoke();
            gameObject.SetActive(false);
        }
    }
}
