using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitriclawAnimEventRelay : MonoBehaviour
{
    public EnemyBehaviorVitriclaw aiScript;
    public void initiateJump()
    {
        aiScript.initiateJump();
    }
    
    public void endJump()
    {
        aiScript.endJump();
    }
}
