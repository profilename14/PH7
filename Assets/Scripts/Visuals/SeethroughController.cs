using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlatKit;


public class SeethroughController : MonoBehaviour
{
    public GameObject player;
    private LayerMask mask;

    private List<GameObject> hitObjects = new();
    private List<GameObject> seethroughObjHits = new();
    private List<GameObject> objsToRemove = new();

    SeethroughMatSwapper swapper;

    private void Start()
    {
        mask = LayerMask.GetMask("Player", "Obstacles");
    }

    private void Update()
    {
        hitObjects.Clear();
        objsToRemove.Clear();

        foreach (RaycastHit h in Physics.RaycastAll(transform.position, (player.transform.position - transform.position).normalized, Vector3.Distance(player.transform.position, transform.position), mask))
        {
            if (h.collider != null && h.collider.gameObject.CompareTag("Seethrough"))
            {
                //Debug.Log("Object in hits: " + h.collider.gameObject);

                hitObjects.Add(h.collider.gameObject);

                swapper = h.collider.gameObject.GetComponent<SeethroughMatSwapper>();

                if (swapper != null && !swapper.isSeethrough)
                {
                    swapper.StartFade();
                    seethroughObjHits.Add(h.collider.gameObject);
                }
            }
        }

        foreach(GameObject o in seethroughObjHits)
        {
            //Debug.Log("Object in seethroughObjHits: " + o.name);
            swapper = o.GetComponent<SeethroughMatSwapper>();
            if (swapper.isSeethrough && !hitObjects.Contains(o))
            {
                swapper.StartReverseFade();
                objsToRemove.Add(o);
            }
        }

        foreach(GameObject o in objsToRemove)
        {
            //Debug.Log("Object in hitsToRemove: " + o);
            seethroughObjHits.Remove(o);
        }
    }
}
