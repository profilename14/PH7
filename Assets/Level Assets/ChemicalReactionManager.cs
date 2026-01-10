using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemicalReactionManager : MonoBehaviour
{
    public static ChemicalReactionManager instance;

    [SerializeField]
    GameObject saltCrystal;

    [SerializeField]
    GameObject saltPlatform;

    [SerializeField]
    float reactionRadius;

    private LayerMask chemicalMask;

    private LayerMask floorMask;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);

        chemicalMask = LayerMask.GetMask("EffectFields");
        floorMask = LayerMask.GetMask("Default", "Obstacles", "Ground");
    }

    public void DoReaction(Chemical c1, Chemical c2, Vector3 point)
    {
        Collider[] chemicalsInRadius = Physics.OverlapSphere(point, reactionRadius, chemicalMask, QueryTriggerInteraction.Collide);
        if(chemicalsInRadius.Length != 0)
        {
            for(int i = 0; i < chemicalsInRadius.Length; i++)
            {
                Debug.Log("Chemicals to delete: " + chemicalsInRadius[i]);
                chemicalsInRadius[i].gameObject.transform.parent.gameObject.SetActive(false);
            }
        }

        if ((c1 == Chemical.Acidic && c2 == Chemical.Alkaline) || (c1 == Chemical.Alkaline && c2 == Chemical.Acidic))
        {
            RaycastHit hit;

            Physics.Raycast(point + Vector3.up * 5, Vector3.down, out hit, 10, floorMask, QueryTriggerInteraction.Ignore);

            Instantiate(saltCrystal, hit.point, Quaternion.identity);
        }
    }

    public void CreateSaltPlatform(Vector3 point)
    {
        Collider[] chemicalsInRadius = Physics.OverlapSphere(point, reactionRadius, chemicalMask, QueryTriggerInteraction.Collide);
        if (chemicalsInRadius.Length != 0)
        {
            for (int i = 0; i < chemicalsInRadius.Length; i++)
            {
                Debug.Log("Chemicals to delete: " + chemicalsInRadius[i]);
                if(!chemicalsInRadius[i].CompareTag("ChemicalPool")) chemicalsInRadius[i].gameObject.transform.parent.gameObject.SetActive(false);
            }
        }

        Instantiate(saltPlatform, point, Quaternion.identity);
    }

    public void ClearNearbyChemicals(Vector3 point)
    {
        Collider[] chemicalsInRadius = Physics.OverlapSphere(point, reactionRadius, chemicalMask, QueryTriggerInteraction.Collide);
        if (chemicalsInRadius.Length != 0)
        {
            for (int i = 0; i < chemicalsInRadius.Length; i++)
            {
                Debug.Log("Chemicals to delete: " + chemicalsInRadius[i]);
                chemicalsInRadius[i].gameObject.transform.parent.gameObject.SetActive(false);
            }
        }
    }
}
