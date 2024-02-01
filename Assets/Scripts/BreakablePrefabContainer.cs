using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static UnityEngine.GraphicsBuffer;



public class BreakablePrefabContainer : MonoBehaviour
{

    [SerializeField] public GameObject objectToMake;

    public void Break()
    {
        Instantiate(objectToMake, transform.position, Quaternion.identity);

        Debug.Log("Made object!");

        Destroy(gameObject);
    }
}
