using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemicalBlobEmitter : MonoBehaviour
{
    [SerializeField]
    private GameObject chemicalBlob;

    [SerializeField]
    bool canSpawnBlobs = true;

    [SerializeField]
    private float angleRandomSpread;

    [SerializeField]
    private float velocityMin;

    [SerializeField]
    private float velocityMax;

    [SerializeField]
    private bool spawnRepeating;

    [SerializeField]
    private float spawnDelay;

    [SerializeField]
    private int blobCountPerInterval;

    float spawnTimer;

    [SerializeField]
    bool useGlobalEmitAngle;

    [SerializeField]
    Vector3 globalEmitAngle;

    [SerializeField]
    Vector3 offset;

    private void Update()
    {
        if (canSpawnBlobs)
        {
            if (spawnDelay != 0)
            {
                spawnTimer += Time.deltaTime;

                if (spawnTimer >= spawnDelay)
                {
                    SpawnBlobs();

                    if (spawnRepeating)
                    {
                        spawnTimer = 0;
                    }
                    else
                    {
                        canSpawnBlobs = false;
                    }
                }
            }
            else
            {
                SpawnBlobs();

                canSpawnBlobs = false;
            }
        }
    }

    public void SpawnBlobs()
    {
        for (int i = 0; i < blobCountPerInterval; i++)
        {
            Quaternion blobAngle = Quaternion.identity;

            if (!useGlobalEmitAngle)
            {
                blobAngle = Quaternion.Euler(transform.eulerAngles.x + Random.Range(-angleRandomSpread, angleRandomSpread),
                transform.eulerAngles.y + Random.Range(-angleRandomSpread, angleRandomSpread),
                transform.eulerAngles.z + Random.Range(-angleRandomSpread, angleRandomSpread));
            }
            else
            {
                blobAngle = Quaternion.Euler(globalEmitAngle.x + Random.Range(-angleRandomSpread, angleRandomSpread),
                globalEmitAngle.y + Random.Range(-angleRandomSpread, angleRandomSpread),
                globalEmitAngle.z + Random.Range(-angleRandomSpread, angleRandomSpread));
            }

            GameObject blob = Instantiate(chemicalBlob, transform.position + offset, blobAngle);
            blob.GetComponent<Rigidbody>().velocity = blob.transform.forward * Random.Range(velocityMin, velocityMax);
        }
    }
}
