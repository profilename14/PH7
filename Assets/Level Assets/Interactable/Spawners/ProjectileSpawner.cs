using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public GameObject projectile;
    public Character sender;
    public AttackData projectileData;

    public void SpawnProjectile()
    {
        Instantiate(projectile, this.transform.position, this.transform.rotation).GetComponent<MyProjectile>().InitProjectile(this.transform.position, this.transform.rotation, sender, projectileData);
    }
}
