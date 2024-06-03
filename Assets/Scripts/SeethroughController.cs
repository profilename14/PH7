using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlatKit;


public class SeethroughController : MonoBehaviour
{
    public GameObject player;
    private LayerMask mask;

    private List<RaycastHit> hits = new();
    private List<RaycastHit> seethroughObjHits = new();

    SeethroughMatSwapper swapper;

    private void Start()
    {
        mask = LayerMask.GetMask("Player", "Obstacles");
    }

    private void Update()
    {
        hits.Clear();
        hits.AddRange(Physics.RaycastAll(transform.position, (player.transform.position - transform.position).normalized, mask));

        foreach(RaycastHit h in hits)
        {
            if (h.collider != null && h.collider.gameObject.CompareTag("Seethrough"))
            {
                swapper = h.collider.gameObject.GetComponent<SeethroughMatSwapper>();
                
                if(swapper != null && !swapper.isSeethrough && !seethroughObjHits.Contains(h))
                {
                    swapper.MakeSeethrough();
                    seethroughObjHits.Add(h);
                }
            }
        }

        foreach(RaycastHit h in seethroughObjHits)
        {
            if(!hits.Contains(h))
            {
                h.collider.gameObject.GetComponent<SeethroughMatSwapper>().ResetMaterial();
            }
        }
    }
}
