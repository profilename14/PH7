using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitboxHandler : MonoBehaviour
{
    [SerializeField]
    AttackState redirectHitboxTo;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player!");

            redirectHitboxTo.RedirectedHitbox(other);
        }
    }
}
