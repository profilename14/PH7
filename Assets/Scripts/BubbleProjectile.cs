using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleProjectile : MonoBehaviour
{
    [SerializeField] private float lifespan = 1.5f;
    [SerializeField] private float lifespanTimer = 0f;
    [SerializeField] private float speed = 10f;

    [SerializeField] private float strength;

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
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(0, 0, 0, 0, transform.position - this.transform.forward);
            other.gameObject.GetComponent<EnemyBehavior>().Neutralize(strength);
            //Destroy(gameObject);
        } else if (other.gameObject.CompareTag("Switch")) {
            other.gameObject.GetComponent<Switch>().Toggle();
            //Destroy(gameObject);
        } else if (other.gameObject.CompareTag("AllowsBubble") || other.gameObject.CompareTag("Player")) {
            //
        } else {
          //Destroy(gameObject);
        }

    }
    void OnColliderEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            //other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(0, 0.66f, 17.5f, transform.position - this.transform.forward);
            //Destroy(gameObject);
        } else if (other.gameObject.CompareTag("Switch")) {
            other.gameObject.GetComponent<Switch>().Toggle();
        } else if (other.gameObject.CompareTag("AllowsBubble") || other.gameObject.CompareTag("Player")) {
            //
        } else {
          //Destroy(gameObject);
        }

    }
}
