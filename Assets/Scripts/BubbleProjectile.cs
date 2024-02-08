using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleProjectile : MonoBehaviour
{
    [SerializeField] private float lifespan = 1.5f;
    [SerializeField] private float lifespanTimer = 0f;
    [SerializeField] private float speed = 10f;


    // Start is called before the first frame update
    void Start()
    {
      lifespanTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        lifespanTimer += Time.deltaTime;
        if (lifespanTimer > lifespan) {
          Destroy(gameObject);
        }
        transform.position += this.transform.forward * Time.deltaTime * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(0, 0.5f, 0.75f, transform.position);
            Destroy(gameObject);
        } else if (other.gameObject.CompareTag("Switch")) {
            other.gameObject.GetComponent<Switch>().Toggle();
            Destroy(gameObject);
        } else if (other.gameObject.CompareTag("AllowsBubble") || other.gameObject.CompareTag("Player")) {
            //
        } else {
          Destroy(gameObject);
        }

    }
    void OnColliderEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(0, 1.5f, 5f, transform.position);
            Destroy(gameObject);
        } else if (other.gameObject.CompareTag("Switch")) {
            other.gameObject.GetComponent<Switch>().Toggle();
        } else if (other.gameObject.CompareTag("AllowsBubble") || other.gameObject.CompareTag("Player")) {
            //
        } else {
          Destroy(gameObject);
        }

    }
}
