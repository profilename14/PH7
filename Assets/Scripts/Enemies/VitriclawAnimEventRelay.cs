using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitriclawAnimEventRelay : MonoBehaviour
{
    public VitriclawAI aiScript;

    public void StartAttack(string state)
    {
        aiScript.StartAttack(state);
    }
    
    public void EndAttack()
    {
        aiScript.EndAttack();
    }
}
