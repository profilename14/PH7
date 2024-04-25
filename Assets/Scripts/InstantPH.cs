using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantPH : MonoBehaviour
{
    public float pHChange;
    public float lifetime;

    private void Awake()
    {
        StartCoroutine(DestroySelf());
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerStats>().ph = Mathf.Clamp(other.gameObject.GetComponent<PlayerStats>().ph + pHChange, 0, 14);
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            if (other.gameObject.GetComponent<EnemyAI>() != null) {
                other.gameObject.GetComponent<EnemyAI>().TakeDamage(0, pHChange, 0, transform.position);
            } else {
                other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(0, pHChange, 0, transform.position);
            }
        }
        if (other.gameObject.tag == "HasPH")
        {
            other.gameObject.GetComponent<ObjectWithPH>().ChangePH(pHChange);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
      if (other.gameObject.tag == "HasPH")
      {
          other.gameObject.GetComponent<ObjectWithPH>().ChangePH(pHChange);
      }
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(lifetime);

        Destroy(this.gameObject);
    }
}
