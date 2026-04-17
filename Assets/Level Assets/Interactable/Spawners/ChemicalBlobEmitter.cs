using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class ChemicalBlobEmitter : MonoBehaviour
{
    [SerializeField]
    private GameObject chemicalBlob;

    public bool canSpawnBlobs = true;

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
    private bool spawnOnAwake;

    [SerializeField]
    private int blobCountPerInterval;

    float spawnTimer;

    [SerializeField]
    bool useGlobalEmitAngle;

    [SerializeField]
    Vector3 globalEmitAngle;

    [SerializeField]
    Axis emitAxis;

    [SerializeField]
    Vector3 offset;

    private void Awake()
    {
        if (spawnOnAwake) SpawnBlobs();
    }

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
                /*switch (emitAxis)
                {
                    case Axis.Forward:
                        blobAngle = Quaternion.Euler(transform.forward.z + Random.Range(-angleRandomSpread, angleRandomSpread),
                        break;
                    default:
                        blobAngle = Quaternion.Euler(transform.eulerAngles.x + Random.Range(-angleRandomSpread, angleRandomSpread),
                        break;
                }*/

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

            if(!useGlobalEmitAngle)
            {
                switch (emitAxis)
                {
                    case Axis.Forward:
                        blob.transform.forward = transform.forward;
                        blob.transform.rotation = Quaternion.Euler(blob.transform.eulerAngles.x + Random.Range(-angleRandomSpread, angleRandomSpread),
                            blob.transform.eulerAngles.y + Random.Range(-angleRandomSpread, angleRandomSpread),
                            blob.transform.eulerAngles.z + Random.Range(-angleRandomSpread, angleRandomSpread));
                        break;
                    default:
                        //blobAngle = Quaternion.Euler(transform.eulerAngles.x + Random.Range(-angleRandomSpread, angleRandomSpread),
                        break;
                }
            }

            blob.GetComponent<Rigidbody>().velocity = blob.transform.forward * Random.Range(velocityMin, velocityMax);
        }
    }

    public void SetCanSpawnBlobs(bool set)
    {
        canSpawnBlobs = set;
    }
}
