using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class CurrentZone : MonoBehaviour
{
    [SerializeField]
    private float currentStrength;

    [SerializeField]
    private float currentStrengthForPlayer;

    private Dictionary<Collider, Rigidbody> entitiesInCurrent = new();

    private void Awake()
    {
        entitiesInCurrent.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Bubble") && !other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy")) return;

        if (!entitiesInCurrent.ContainsKey(other)) entitiesInCurrent.Add(other, other.gameObject.GetComponentInParentOrChildren<Rigidbody>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Bubble") && !other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy")) return;

        if (entitiesInCurrent.ContainsKey(other)) entitiesInCurrent.Remove(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag("Bubble") && !other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy")) return;

        if (entitiesInCurrent.ContainsKey(other))
        {
            if (other.gameObject.CompareTag("Bubble")) entitiesInCurrent[other].AddForce(this.gameObject.transform.forward * currentStrength, ForceMode.Force);
            else if (other.gameObject.CompareTag("Player")) Player.instance.movementController.AddVelocity(this.gameObject.transform.forward * currentStrengthForPlayer);
        }
    }
}
