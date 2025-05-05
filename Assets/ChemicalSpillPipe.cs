using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemicalSpillPipe : MonoBehaviour
{
    Chemical ColliderEffectType;

    [SerializeField]
    private float timeTurnedOff;

    [SerializeField]
    private float timeTurnedOn;

    [SerializeField]
    private ParticleSystem[] chemicalEffects;

    [SerializeField]
    private GameObject colliderObj;

    private void Awake()
    {
        StartCoroutine(TurnOnAndWait());
    }

    public IEnumerator TurnOnAndWait()
    {
        foreach (ParticleSystem p in chemicalEffects)
        {
            p.Play();
        }

        colliderObj.SetActive(true);

        yield return new WaitForSeconds(timeTurnedOn);

        StartCoroutine(StopAndWait());
    }

    public IEnumerator StopAndWait()
    {
        foreach (ParticleSystem p in chemicalEffects)
        {
            p.Stop();
        }
        colliderObj.SetActive(false);

        yield return new WaitForSeconds(timeTurnedOff);
        StartCoroutine(TurnOnAndWait());
    }

    public void OnTriggerEnter(Collider other)
    {
        
    }
}
