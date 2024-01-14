using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatController : MonoBehaviour
{
    EnemyStats stats;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damage, float ph, float knockback, Vector3 sourcePos)
    {
        stats.health -= damage;
        stats.ph += ph;

        Vector3 dir = -((sourcePos - transform.position).normalized);
        Vector3 velocity = dir * knockback;

        rb.AddForce(velocity, ForceMode.Impulse);

        if (stats.health <= 0) Destroy(this.gameObject);
    }
}
